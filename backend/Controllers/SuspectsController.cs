using CriminalNetwork.Api.Data;
using CriminalNetwork.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CriminalNetwork.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuspectsController : ControllerBase
{
    private readonly AppDbContext _database;

    public SuspectsController(AppDbContext database)
    {
        _database = database;
    }

    // Returns all suspects so the frontend can display them.
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var suspects = await _database.Suspects
            .AsNoTracking()
            .OrderBy(suspect => suspect.Id)
            .ToListAsync();

        return Ok(suspects);
    }

    // Saves a new suspect sent from the frontend form.
    [HttpPost]
    public async Task<IActionResult> Create(CreateSuspectRequest request)
    {
        var name = request.Name.Trim();
        var nameIsValid = name.All(character => char.IsLetter(character)
            || character == ' ' || character == '-');

        if (name == "" || !nameIsValid)
        {
            return BadRequest(new { message = "The name must only contain letters, spaces or hyphens." });
        }

        if (request.DateOfBirth > DateOnly.FromDateTime(DateTime.Today))
        {
            return BadRequest(new { message = "The date of birth cannot be in the future." });
        }

        if (request.RiskLevel < 1 || request.RiskLevel > 5)
        {
            return BadRequest(new { message = "The risk level must be between 1 and 5." });
        }

        var duplicateExists = await _database.Suspects.AnyAsync(suspect =>
            suspect.Name.ToLower() == name.ToLower()
            && suspect.DateOfBirth == request.DateOfBirth);

        if (duplicateExists)
        {
            return BadRequest(new { message = "A suspect with this name and date of birth already exists." });
        }

        var suspect = new Suspect
        {
            Name = name,
            DateOfBirth = request.DateOfBirth,
            RiskLevel = request.RiskLevel
        };

        _database.Suspects.Add(suspect);
        await _database.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = suspect.Id }, suspect);
    }

    // Updates an existing suspect sent from the edit form.
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreateSuspectRequest request)
    {
        var suspect = await _database.Suspects.FindAsync(id);

        if (suspect is null)
        {
            return NotFound(new { message = "The suspect could not be found." });
        }

        var name = request.Name.Trim();
        var nameIsValid = name.All(character => char.IsLetter(character)
            || character == ' ' || character == '-');

        if (name == "" || !nameIsValid)
        {
            return BadRequest(new { message = "The name must only contain letters, spaces or hyphens." });
        }

        if (request.DateOfBirth > DateOnly.FromDateTime(DateTime.Today))
        {
            return BadRequest(new { message = "The date of birth cannot be in the future." });
        }

        if (request.RiskLevel < 1 || request.RiskLevel > 5)
        {
            return BadRequest(new { message = "The risk level must be between 1 and 5." });
        }

        var duplicateExists = await _database.Suspects.AnyAsync(item =>
            item.Id != id
            && item.Name.ToLower() == name.ToLower()
            && item.DateOfBirth == request.DateOfBirth);

        if (duplicateExists)
        {
            return BadRequest(new { message = "A suspect with this name and date of birth already exists." });
        }

        suspect.Name = name;
        suspect.DateOfBirth = request.DateOfBirth;
        suspect.RiskLevel = request.RiskLevel;
        await _database.SaveChangesAsync();

        return Ok(suspect);
    }
}
