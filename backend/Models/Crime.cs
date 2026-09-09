namespace CriminalNetwork.Api.Models;

public class Crime
{
    public int Id { get; set; }
    public string Type { get; set; } = "";
    public DateOnly Date { get; set; }
    public string Location { get; set; } = "";
}
