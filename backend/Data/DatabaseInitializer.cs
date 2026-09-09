using CriminalNetwork.Api.Models;

namespace CriminalNetwork.Api.Data;

// Adds the same starter records currently shown by the frontend.
public static class DatabaseInitializer
{
    public static void Seed(AppDbContext database)
    {
        if (!database.Suspects.Any())
        {
            database.Suspects.AddRange(
                new Suspect
                {
                    Id = 1001,
                    Name = "John Ham",
                    DateOfBirth = new DateOnly(1990, 2, 11),
                    RiskLevel = 3
                },
                new Suspect
                {
                    Id = 1002,
                    Name = "Sarah Jones",
                    DateOfBirth = new DateOnly(1985, 7, 22),
                    RiskLevel = 5
                });
        }

        if (!database.Crimes.Any())
        {
            database.Crimes.AddRange(
                new Crime
                {
                    Id = 201,
                    Type = "Burglary",
                    Date = new DateOnly(2026, 1, 14),
                    Location = "Manchester"
                },
                new Crime
                {
                    Id = 202,
                    Type = "Fraud",
                    Date = new DateOnly(2026, 2, 3),
                    Location = "London"
                });
        }

        if (!database.Relationships.Any())
        {
            database.Relationships.Add(new Relationship
            {
                Id = 301,
                FirstSuspectId = 1001,
                SecondSuspectId = 1002,
                Type = "Associate"
            });
        }

        database.SaveChanges();
    }
}
