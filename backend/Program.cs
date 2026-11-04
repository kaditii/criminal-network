using CriminalNetwork.Api.Data;
using CriminalNetwork.Api.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Uses a local SQLite file so the data stays after the program closes.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Cases") ?? "Data Source=criminal-network.db"));

builder.Services.AddControllers(options =>
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.ResponseCacheAttribute { NoStore = true }));
builder.Services.AddScoped<IPasswordHasher<Detective>, PasswordHasher<Detective>>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "DetectiveSession";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = false;
        // APIs should return a status code instead of redirecting to an HTML page.
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = 403;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization(options =>
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "local", _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Criminal Network API",
        Version = "v1",
        Description = "API for managing suspects, crimes, and relationships."
    });
});

var app = builder.Build();

// Creates the tables the first time the application is started.
using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    database.Database.EnsureCreated();
    DatabaseInitializer.AddCrimeDetails(database);
    DatabaseInitializer.AddDetectiveTable(database);
    DatabaseInitializer.Seed(database);
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Criminal Network API v1");
    options.DocumentTitle = "Criminal Network API";
    options.UseRequestInterceptor("(request) => { request.headers['X-Requested-With'] = 'CriminalNetwork'; return request; }");
});

// The frontend and API use one server, so no CORS configuration is needed.
var frontend = new PhysicalFileProvider(Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "../frontend")));
app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = frontend });
app.UseStaticFiles(new StaticFileOptions { FileProvider = frontend });
app.UseRouting();
// Browsers cannot send this custom header cross-origin without CORS approval.
// Requiring it on writes prevents another website from submitting cookie-authenticated forms.
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api")
        && !HttpMethods.IsGet(context.Request.Method)
        && !HttpMethods.IsHead(context.Request.Method)
        && !HttpMethods.IsOptions(context.Request.Method)
        && context.Request.Headers["X-Requested-With"] != "CriminalNetwork")
    {
        context.Response.StatusCode = 403;
        await context.Response.WriteAsJsonAsync(new { message = "Missing request verification header." });
        return;
    }
    await next();
});
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

app.Run();
