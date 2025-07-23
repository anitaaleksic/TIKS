using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyHotelApp.server.Models;
using System.ComponentModel.DataAnnotations;

namespace MyHotelApp.Controllers;


[ApiController]
[Route("api/[controller]")]
public class ReservationController : ControllerBase
{
    private readonly HotelContext _context;

    public ReservationController(HotelContext context)
    {
        _context = context;
    }

    [HttpPost("CreateReservation")]
    public async Task<IActionResult> CreateReservation([FromBody] Reservation reservation)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (reservation.RoomNumber <= 0)
            {
                return BadRequest("Room number must be a positive integer.");
            }
            if (reservation.CheckInDate >= reservation.CheckOutDate)
            {
                return BadRequest("Check-out date must be after check-in date.");
            }
            if (reservation.TotalPrice < 0)
            {
                return BadRequest("Total price must be a non-negative value.");
            }
            if (!await _context.Rooms.AnyAsync(r => r.RoomNumber == reservation.RoomNumber))
            {
                return BadRequest($"Room with number {reservation.RoomNumber} does not exist.");
            }
            if (!await _context.Guests.AnyAsync(g => g.JMBG == reservation.GuestID))
            {
                return BadRequest($"Guest with ID {reservation.GuestID} does not exist.");
            }

            var existingReservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.RoomNumber == reservation.RoomNumber &&
                                          r.CheckInDate < reservation.CheckOutDate &&
                                          r.CheckOutDate > reservation.CheckInDate);

            if (existingReservation != null)
            {
                return BadRequest("The room is already reserved for the selected dates.");
            }

            Reservation newReservation = new Reservation
            {
                RoomNumber = reservation.RoomNumber,
                GuestID = reservation.GuestID,
                CheckInDate = reservation.CheckInDate,
                CheckOutDate = reservation.CheckOutDate,
                TotalPrice = reservation.TotalPrice
            };

            await _context.Reservations.AddAsync(newReservation);
            await _context.SaveChangesAsync();
            return Ok($"Reservation created successfully for room {newReservation.RoomNumber}.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("GetAllReservations")]
    public async Task<IActionResult> GetAllReservations()
    {
        try
        {
            var reservations = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.Guest)
                .ToListAsync();
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpGet("GetReservationById/{id}")]
    public async Task<IActionResult> GetReservationById(int id)
    {
        try
        {
            var reservation = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.Guest)
                .FirstOrDefaultAsync(r => r.ReservationID == id);
            if (reservation == null)
            {
                return NotFound($"Reservation with ID {id} not found.");
            }
            return Ok(reservation);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("UpdateReservation/{id}")]
    public async Task<IActionResult> UpdateReservation(int id, [FromBody] Reservation reservation)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (reservation.RoomNumber <= 0)
            {
                return BadRequest("Room number must be a positive integer.");
            }
            if (reservation.CheckInDate >= reservation.CheckOutDate)
            {
                return BadRequest("Check-out date must be after check-in date.");
            }
            if (reservation.TotalPrice <= 0)
            {
                return BadRequest("Total price must be a non-negative value.");
            }
            if (!await _context.Rooms.AnyAsync(r => r.RoomNumber == reservation.RoomNumber))
            {
                return BadRequest($"Room with number {reservation.RoomNumber} does not exist.");
            }
            if (!await _context.Guests.AnyAsync(g => g.JMBG == reservation.GuestID))
            {
                return BadRequest($"Guest with ID {reservation.GuestID} does not exist.");
            }

            var existingReservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.ReservationID == id);
            if (existingReservation == null)
            {
                return NotFound($"Reservation with ID {id} not found.");
            }

            existingReservation.RoomNumber = reservation.RoomNumber;
            existingReservation.GuestID = reservation.GuestID;
            existingReservation.CheckInDate = reservation.CheckInDate;
            existingReservation.CheckOutDate = reservation.CheckOutDate;
            existingReservation.TotalPrice = reservation.TotalPrice;

            await _context.SaveChangesAsync();
            return Ok($"Reservation with ID {id} updated successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpDelete("DeleteReservation/{id}")]
    public async Task<IActionResult> DeleteReservation(int id)
    {
        try
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound($"Reservation with ID {id} not found.");
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return Ok($"Reservation with ID {id} deleted successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

}