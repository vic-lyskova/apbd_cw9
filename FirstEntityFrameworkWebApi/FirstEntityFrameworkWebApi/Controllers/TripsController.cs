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
            Description = t.Description,
            DateFrom = t.DateFrom,
            DateTo = t.DateTo,
            MaxPeople = t.MaxPeople,
            Countries = t.IdCountries.Select(c => new
            {
                Name = c.Name
            }),
            Clients = t.ClientTrips.Select(clt => _context.Clients.Where(cl => cl.IdClient == clt.IdClient))
                .Select(cl => new
                {
                    FirstName = cl.First().FirstName,
                    LastName = cl.First().LastName
                })
        })
        .OrderByDescending(t => t.DateFrom)
            .ToListAsync();

        return Ok(trips);
    }

    [HttpDelete("{idClient:int}")]
    public async Task<IActionResult> RemoveClient(int idClient)
    {
        if (_context.Clients
            .Where(cl => cl.IdClient == idClient)
            .Any(cl => _context.ClientTrips.Any(clt => cl.IdClient == clt.IdClient)))
        {
            return BadRequest("Client has trips");
        }

        _context.Clients.Remove(_context.Clients.First(cl => cl.IdClient == idClient));
        await _context.SaveChangesAsync();
        return Ok();
    }
}