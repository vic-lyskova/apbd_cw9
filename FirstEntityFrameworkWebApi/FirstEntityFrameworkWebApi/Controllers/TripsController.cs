using FirstEntityFrameworkWebApi.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirstEntityFrameworkWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly ApbdContext _context;
    public TripsController(ApbdContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetTrips()
    {
        // var trips = await _context.Trips.ToListAsync();

        var trips = await _context.Trips.Select(t => new
        {
            Name = t.Name,
            Countrie = t.IdCountries.Select(c => new
            {
                Name = c.Name
            })
        }).ToListAsync();

        return Ok(trips);
    }
}