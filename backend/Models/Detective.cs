namespace CriminalNetwork.Api.Models;

public class Detective
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string NormalizedUsername { get; set; } = "";
    // Store the salted hash, never the original password.
    public string PasswordHash { get; set; } = "";
}
