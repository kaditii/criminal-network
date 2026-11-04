using CriminalNetwork.Api.Data;
using CriminalNetwork.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CriminalNetwork.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CrimesController : ControllerBase
{
    private readonly AppDbContext _database;

    public CrimesController(AppDbContext database)
    {
        _database = database;
    }

    // Returns all crimes so the frontend can display them.
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var crimes = await _database.Crimes
            .AsNoTracking()
            .OrderBy(crime => crime.Id)
            .ToListAsync();

        var links = await _database.CrimeSuspects.AsNoTracking().ToListAsync();
        return Ok(crimes.Select(crime => CrimeDetails(crime,
            links.Where(link => link.CrimeId == crime.Id).Select(link => link.SuspectId).ToList())));
    }

    // Saves a new crime sent from the frontend form.
    [HttpPost]
    public async Task<IActionResult> Create(CreateCrimeRequest request)
    {
        if (!await ValidSuspects(request))
            return BadRequest(new { message = "Every linked suspect must exist." });
        var type = request.Type.Trim();
        var location = request.Location.Trim();

        if (type == "")
        {
            return BadRequest(new { message = "The crime type is required." });
        }

        if (request.Date > DateOnly.FromDateTime(DateTime.Today))
        {
            return BadRequest(new { message = "The crime date cannot be in the future." });
        }

        if (location == "")
        {
            return BadRequest(new { message = "The location is required." });
        }

        var duplicateExists = await _database.Crimes.AnyAsync(crime =>
            crime.Type.ToLower() == type.ToLower()
            && crime.Date == request.Date
            && crime.Location.ToLower() == location.ToLower());

        if (duplicateExists)
        {
            return BadRequest(new { message = "This crime record already exists." });
        }

        var crime = new Crime
        {
            Type = type,
            Date = request.Date,
            Location = location,
            Witnesses = (request.Witnesses ?? "").Trim(),
            Statements = (request.Statements ?? "").Trim()
        };

        // Save the crime and its links together so a partial record is never left behind.
        using var transaction = await _database.Database.BeginTransactionAsync();
        _database.Crimes.Add(crime);
        await _database.SaveChangesAsync();
        AddSuspectLinks(crime.Id, request.SuspectIds);
        await _database.SaveChangesAsync();
        await transaction.CommitAsync();

        return Created($"/api/crimes/{crime.Id}", CrimeDetails(crime, request.SuspectIds));
    }

    // Updates an existing crime sent from the edit form.
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreateCrimeRequest request)
    {
        if (!await ValidSuspects(request))
            return BadRequest(new { message = "Every linked suspect must exist." });
        var crime = await _database.Crimes.FindAsync(id);

        if (crime is null)
        {
            return NotFound(new { message = "The crime could not be found." });
        }

        var type = request.Type.Trim();
        var location = request.Location.Trim();

        if (type == "")
        {
            return BadRequest(new { message = "The crime type is required." });
        }

        if (request.Date > DateOnly.FromDateTime(DateTime.Today))
        {
            return BadRequest(new { message = "The crime date cannot be in the future." });
        }

        if (location == "")
        {
            return BadRequest(new { message = "The location is required." });
        }

        var duplicateExists = await _database.Crimes.AnyAsync(item =>
            item.Id != id
            && item.Type.ToLower() == type.ToLower()
            && item.Date == request.Date
            && item.Location.ToLower() == location.ToLower());

        if (duplicateExists)
        {
            return BadRequest(new { message = "This crime record already exists." });
        }

        crime.Type = type;
        crime.Date = request.Date;
        crime.Location = location;
        crime.Witnesses = (request.Witnesses ?? "").Trim();
        crime.Statements = (request.Statements ?? "").Trim();
        var oldLinks = await _database.CrimeSuspects.Where(link => link.CrimeId == id).ToListAsync();
        // Keep existing links and only add/remove the ones changed in the form.
        _database.CrimeSuspects.RemoveRange(oldLinks.Where(link => !request.SuspectIds.Contains(link.SuspectId)));
        AddSuspectLinks(id, request.SuspectIds.Where(suspectId => !oldLinks.Any(link => link.SuspectId == suspectId)).ToList());
        await _database.SaveChangesAsync();

        return Ok(CrimeDetails(crime, request.SuspectIds));
    }

    // The existing CrimeSuspects table stores which suspects are linked to a crime.
    private async Task<bool> ValidSuspects(CreateCrimeRequest request)
    {
        request.SuspectIds = (request.SuspectIds ?? new()).Distinct().ToList();
        return await _database.Suspects.CountAsync(s => request.SuspectIds.Contains(s.Id)) == request.SuspectIds.Count;
    }

    private void AddSuspectLinks(int crimeId, List<int> suspectIds)
    {
        foreach (var suspectId in suspectIds)
            _database.CrimeSuspects.Add(new CrimeSuspect { CrimeId = crimeId, SuspectId = suspectId });
    }

    private static object CrimeDetails(Crime crime, List<int> suspectIds) => new
    {
        crime.Id, crime.Type, crime.Date, crime.Location, crime.Witnesses, crime.Statements, suspectIds
    };

    // Deletes a crime and any links stored for it.
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var crime = await _database.Crimes.FindAsync(id);

        if (crime is null)
        {
            return NotFound(new { message = "The crime could not be found." });
        }

        _database.Crimes.Remove(crime);
        await _database.SaveChangesAsync();

        return NoContent();
    }
}
