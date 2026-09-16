namespace CriminalNetwork.Api.Models;

// Holds the values sent from the relationship form.
public class CreateRelationshipRequest
{
    public int FirstSuspectId { get; set; }
    public int SecondSuspectId { get; set; }
    public string Type { get; set; } = "";
}
