using CriminalNetwork.Api.Data;
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
}
