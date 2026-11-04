using CriminalNetwork.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CriminalNetwork.Api.Data;

// Adds the same starter records currently shown by the frontend.
public static class DatabaseInitializer
{
    // Add the new optional fields to older databases without deleting records.
    public static void AddCrimeDetails(AppDbContext database)
    {
        var columns = database.Database.SqlQueryRaw<string>(
            "SELECT name AS Value FROM pragma_table_info('Crimes')").ToList();
        if (!columns.Contains("Witnesses"))
            database.Database.ExecuteSqlRaw("ALTER TABLE Crimes ADD COLUMN Witnesses TEXT NOT NULL DEFAULT '';");
        if (!columns.Contains("Statements"))
            database.Database.ExecuteSqlRaw("ALTER TABLE Crimes ADD COLUMN Statements TEXT NOT NULL DEFAULT '';");
    }

    // EnsureCreated cannot add tables to an existing database. This repeatable
    // schema upgrade adds accounts without replacing any case records.
    public static void AddDetectiveTable(AppDbContext database)
    {
        database.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS Detectives (
                Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL,
                NormalizedUsername TEXT NOT NULL,
                PasswordHash TEXT NOT NULL);
            CREATE UNIQUE INDEX IF NOT EXISTS IX_Detectives_NormalizedUsername
                ON Detectives (NormalizedUsername);
            """);
    }

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
