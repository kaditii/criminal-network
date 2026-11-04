namespace CriminalNetwork.Api.Models;

// Holds the values sent from the crime form.
public class CreateCrimeRequest
{
    public string Type { get; set; } = "";
    public DateOnly Date { get; set; }
    public string Location { get; set; } = "";
    public string Witnesses { get; set; } = "";
    public string Statements { get; set; } = "";
    public List<int> SuspectIds { get; set; } = new();
}
