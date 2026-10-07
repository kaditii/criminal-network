using CriminalNetwork.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Uses a local SQLite file so the data stays after the program closes.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=criminal-network.db"));

builder.Services.AddControllers();
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
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Creates the tables the first time the application is started.
using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    database.Database.EnsureCreated();
    DatabaseInitializer.Seed(database);
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Criminal Network API v1");
    options.DocumentTitle = "Criminal Network API";
});

app.UseCors("frontend");
app.MapControllers();

app.Run();
