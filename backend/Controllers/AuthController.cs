using System.Security.Claims;
using CriminalNetwork.Api.Data;
using CriminalNetwork.Api.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CriminalNetwork.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _database;
    private readonly IPasswordHasher<Detective> _hasher;

    public AuthController(AppDbContext database, IPasswordHasher<Detective> hasher)
    {
        _database = database;
        _hasher = hasher;
    }

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("register")]
    public async Task<IActionResult> Register(AuthRequest request)
    {
        var normalizedUsername = request.Username.ToUpperInvariant();
        if (await _database.Detectives.AnyAsync(d => d.NormalizedUsername == normalizedUsername))
            return Conflict(new { message = "That username is already taken." });

        var detective = new Detective
        {
            Username = request.Username,
            NormalizedUsername = normalizedUsername
        };
        // HashPassword generates a random salt and includes it in the stored hash.
        detective.PasswordHash = _hasher.HashPassword(detective, request.Password);
        _database.Detectives.Add(detective);
        try
        {
            await _database.SaveChangesAsync();
        }
        catch (DbUpdateException error) when (error.InnerException is SqliteException { SqliteErrorCode: 19 })
        {
            return Conflict(new { message = "That username is already taken." });
        }

        return StatusCode(201, new { message = "Account created. You can now log in." });
    }

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(AuthRequest request)
    {
        var normalizedUsername = request.Username.ToUpperInvariant();
        var detective = await _database.Detectives.SingleOrDefaultAsync(
            d => d.NormalizedUsername == normalizedUsername);
        if (detective is null)
            return Unauthorized(new { message = "Username or password is incorrect." });

        // Verify using the salt in the saved hash; hashing again would use a new salt.
        var result = _hasher.VerifyHashedPassword(detective, detective.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Username or password is incorrect." });

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, detective.Id.ToString()),
            new Claim(ClaimTypes.Name, detective.Username)
        }, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
        return Ok(new { username = detective.Username });
    }

    [HttpGet("me")]
    public IActionResult Me() => Ok(new { username = User.Identity!.Name });

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }
}
