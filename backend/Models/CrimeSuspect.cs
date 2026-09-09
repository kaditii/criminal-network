namespace CriminalNetwork.Api.Models;

// Links a suspect to a crime without repeating either record's details.
public class CrimeSuspect
{
    public int Id { get; set; }
    public int CrimeId { get; set; }
    public int SuspectId { get; set; }

    public Crime? Crime { get; set; }
    public Suspect? Suspect { get; set; }
}
