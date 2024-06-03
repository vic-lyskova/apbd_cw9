using FirstEntityFrameworkWebApi.Context;
using FirstEntityFrameworkWebApi.DTOs;
using FirstEntityFrameworkWebApi.Models;
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

    [HttpPost("/api/[controller]/{idTrip:int}/clients")]
    public async Task<IActionResult> AddClientToTrip(int idTrip, AddClientToTripDTO addClientToTripDto)
    {
        var client = _context.Clients.First(cl => cl.Pesel.Equals(addClientToTripDto.Pesel));
        if (_context.Clients.Any(cl => cl.Pesel.Equals(addClientToTripDto.Pesel)))
        {
            return BadRequest("Client already exists");
        }

        if (_context.ClientTrips.Any(clt => clt.IdTrip == idTrip))
        {
            return BadRequest("Client already booked the trip");
        }

        if (!_context.Trips.Any(t => t.IdTrip == addClientToTripDto.IdTrip))
        {
            return NotFound("Trip doesn't exist");
        }

        if (_context.Trips.First(t => t.IdTrip == addClientToTripDto.IdTrip).DateFrom < DateTime.Today)
        {
            return NotFound("The trip was in  the past");
        }

        _context.ClientTrips.Add(new ClientTrip
        {
            IdClient = client.IdClient,
            IdTrip = idTrip,
            RegisteredAt = DateTime.Now,
            PaymentDate = DateTime.Parse(addClientToTripDto.PaymentDate)
        });

        await _context.SaveChangesAsync();
        return Ok();
    }
}