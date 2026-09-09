namespace CriminalNetwork.Api.Models;

// Holds the values sent from the add suspect form.
public class CreateSuspectRequest
{
    public string Name { get; set; } = "";
    public DateOnly DateOfBirth { get; set; }
    public int RiskLevel { get; set; }
}
