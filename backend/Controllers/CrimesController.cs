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

        return Ok(crimes);
    }

    // Saves a new crime sent from the frontend form.
    [HttpPost]
    public async Task<IActionResult> Create(CreateCrimeRequest request)
    {
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
            Location = location
        };

        _database.Crimes.Add(crime);
        await _database.SaveChangesAsync();

        return Created($"/api/crimes/{crime.Id}", crime);
    }

    // Updates an existing crime sent from the edit form.
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreateCrimeRequest request)
    {
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
        await _database.SaveChangesAsync();

        return Ok(crime);
    }

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
