using CriminalNetwork.Api.Data;
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
}
