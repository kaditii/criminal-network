namespace CriminalNetwork.Api.Models;

public class Relationship
{
    public int Id { get; set; }
    public int FirstSuspectId { get; set; }
    public int SecondSuspectId { get; set; }
    public string Type { get; set; } = "";

    public Suspect? FirstSuspect { get; set; }
    public Suspect? SecondSuspect { get; set; }
}
