namespace CriminalNetwork.Api.Models;

public class Suspect
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateOnly DateOfBirth { get; set; }
    public int RiskLevel { get; set; }
}
