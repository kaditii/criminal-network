using CriminalNetwork.Api.Data;
using CriminalNetwork.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CriminalNetwork.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RelationshipsController : ControllerBase
{
    private readonly AppDbContext _database;

    public RelationshipsController(AppDbContext database)
    {
        _database = database;
    }

    // Returns all relationships so the frontend can display them.
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var relationships = await _database.Relationships
            .AsNoTracking()
            .OrderBy(relationship => relationship.Id)
            .ToListAsync();

        return Ok(relationships);
    }

    // Saves a new relationship sent from the frontend form.
    [HttpPost]
    public async Task<IActionResult> Create(CreateRelationshipRequest request)
    {
        var errorMessage = await ValidateRequest(request, null);

        if (errorMessage is not null)
        {
            return BadRequest(new { message = errorMessage });
        }

        var relationship = new Relationship
        {
            FirstSuspectId = request.FirstSuspectId,
            SecondSuspectId = request.SecondSuspectId,
            Type = request.Type.Trim()
        };

        _database.Relationships.Add(relationship);
        await _database.SaveChangesAsync();

        return Created($"/api/relationships/{relationship.Id}", relationship);
    }

    // Updates an existing relationship sent from the edit form.
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreateRelationshipRequest request)
    {
        var relationship = await _database.Relationships.FindAsync(id);

        if (relationship is null)
        {
            return NotFound(new { message = "The relationship could not be found." });
        }

        var errorMessage = await ValidateRequest(request, id);

        if (errorMessage is not null)
        {
            return BadRequest(new { message = errorMessage });
        }

        relationship.FirstSuspectId = request.FirstSuspectId;
        relationship.SecondSuspectId = request.SecondSuspectId;
        relationship.Type = request.Type.Trim();
        await _database.SaveChangesAsync();

        return Ok(relationship);
    }

    // Deletes a relationship from the database.
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var relationship = await _database.Relationships.FindAsync(id);

        if (relationship is null)
        {
            return NotFound(new { message = "The relationship could not be found." });
        }

        _database.Relationships.Remove(relationship);
        await _database.SaveChangesAsync();

        return NoContent();
    }

    // Checks that the relationship links two valid and different suspects.
    private async Task<string?> ValidateRequest(CreateRelationshipRequest request, int? relationshipId)
    {
        var type = request.Type.Trim();

        if (type == "")
        {
            return "The relationship type is required.";
        }

        if (request.FirstSuspectId == request.SecondSuspectId)
        {
            return "Choose two different suspects.";
        }

        var suspectIds = new[] { request.FirstSuspectId, request.SecondSuspectId };
        var suspectCount = await _database.Suspects.CountAsync(suspect => suspectIds.Contains(suspect.Id));

        if (suspectCount != 2)
        {
            return "Both suspects must exist.";
        }

        var duplicateExists = await _database.Relationships.AnyAsync(relationship =>
            relationship.Id != relationshipId
            && relationship.Type.ToLower() == type.ToLower()
            && ((relationship.FirstSuspectId == request.FirstSuspectId
                    && relationship.SecondSuspectId == request.SecondSuspectId)
                || (relationship.FirstSuspectId == request.SecondSuspectId
                    && relationship.SecondSuspectId == request.FirstSuspectId)));

        if (duplicateExists)
        {
            return "This relationship already exists.";
        }

        return null;
    }
}
