using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyHotelApp.server.Models;
using System.ComponentModel.DataAnnotations;

namespace MyHotelApp.Controllers;


[ApiController]
[Route("api/[controller]")]
public class GuestController : ControllerBase
{
    private readonly HotelContext _context;

    public GuestController(HotelContext context)
    {
        _context = context;
    }

    [HttpPost("CreateGuest")]
    public async Task<IActionResult> CreateGuest([FromBody] GuestDTO guest)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var g = await _context.Guests.FirstOrDefaultAsync(g => g.JMBG == guest.JMBG);
            if (g != null)
            {
                return BadRequest($"Guest with the same JMBG ({guest.JMBG}) already exists.");
            }

            if (guest.JMBG.Length != 13)
            {
                return BadRequest("JMBG must be exactly 13 characters long.");
            }
            if (string.IsNullOrEmpty(guest.FullName) || guest.FullName.Length > 100)
            {
                return BadRequest("Full name is required and cannot exceed 100 characters.");
            }
            //is valid proverava razmak, /, -
            if (!string.IsNullOrEmpty(guest.PhoneNumber) && !new PhoneAttribute().IsValid(guest.PhoneNumber))
            {
                return BadRequest("Invalid phone number.");
            }
            if (string.IsNullOrEmpty(guest.PhoneNumber) || guest.PhoneNumber.Length < 12 || guest.PhoneNumber.Length > 13)
            {
                return BadRequest("Phone number is required and must be between 12 and 13 characters long.");
            }

            var newGuest = new Guest
            {
                JMBG = guest.JMBG,
                FullName = guest.FullName,
                PhoneNumber = guest.PhoneNumber
            };

            await _context.Guests.AddAsync(newGuest);
            await _context.SaveChangesAsync();
            return Ok($"Guest with JMBG {newGuest.JMBG} created successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("GetAllGuests")]
    public async Task<IActionResult> GetAllGuests()
    {
        try
        {
            if (_context.Guests == null || !_context.Guests.Any())
            {
                return NotFound("No guests found.");
            }
            var guests = await _context.Guests.ToListAsync();
            return Ok(guests);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

    }

    [HttpGet("GetGuestByJMBG/{jmbg}")]
    public async Task<IActionResult> GetGuestByJMBG(string jmbg)
    {
        try
        {
            if (string.IsNullOrEmpty(jmbg) || jmbg.Length != 13)
            {
                return BadRequest("JMBG must be exactly 13 characters long.");
            }
            var guest = await _context.Guests.FirstOrDefaultAsync(g => g.JMBG == jmbg);
            if (guest == null)
            {
                return NotFound($"Guest with JMBG {jmbg} not found.");
            }
            return Ok(guest);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("UpdateGuest/{jmbg}")]
    public async Task<IActionResult> UpdateGuest(string jmbg, [FromBody] GuestDTO guest)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (jmbg.Length != 13)
            {
                return BadRequest("JMBG must be exactly 13 characters long.");
            }

            var g = await _context.Guests.FirstOrDefaultAsync(g => g.JMBG == jmbg);
            if (g == null)
            {
                return NotFound($"Guest with JMBG {jmbg} not found.");
            }
            if (string.IsNullOrEmpty(guest.FullName) || guest.FullName.Length > 100)
            {
                return BadRequest("Full name is required and cannot exceed 100 characters.");
            }
            if (!string.IsNullOrEmpty(guest.PhoneNumber) && !new PhoneAttribute().IsValid(guest.PhoneNumber))
            {
                return BadRequest("Invalid phone number.");
            }
            if (guest.PhoneNumber.Length < 12 || guest.PhoneNumber.Length > 13)
            {
                return BadRequest("Phone number is required and must be between 12 and 13 characters long.");
            }

            g.FullName = guest.FullName;
            g.PhoneNumber = guest.PhoneNumber;

            await _context.SaveChangesAsync();
            return Ok($"Guest with JMBG {jmbg} updated successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpDelete("DeleteGuest/{jmbg}")]
    public async Task<IActionResult> DeleteGuest(string jmbg)
    {
        try
        {
            var guest = await _context.Guests.Include(g => g.Reservations).FirstOrDefaultAsync(g => g.JMBG == jmbg);
            if (guest == null)
            {
                return NotFound($"Guest with JMBG {jmbg} not found.");
            }
            if (guest.Reservations != null && guest.Reservations.Any())
            {
                return BadRequest("You cannot delete this guest because they have existing reservations. Please delete the reservations first.");
            }

            _context.Reservations.RemoveRange(guest.Reservations);
            _context.Guests.Remove(guest);
            await _context.SaveChangesAsync();
            return Ok($"Guest with JMBG {jmbg} deleted successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }


}