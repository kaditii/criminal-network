using System.ComponentModel.DataAnnotations;

namespace CriminalNetwork.Api.Models;

public class AuthRequest
{
    [Required, RegularExpression(@"^[a-zA-Z0-9_-]{3,30}$",
        ErrorMessage = "Use 3–30 letters, numbers, underscores or hyphens for your username.")]
    public string Username { get; set; } = "";

    [Required, StringLength(128, MinimumLength = 10,
        ErrorMessage = "Use a password between 10 and 128 characters.")]
    public string Password { get; set; } = "";
}
