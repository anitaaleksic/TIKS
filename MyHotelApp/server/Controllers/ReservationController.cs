using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
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
    public async Task<IActionResult> CreateReservation([FromBody] ReservationDTO reservation)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (reservation.RoomNumber < 101 || reservation.RoomNumber > 699)
            {
                return BadRequest("Room number must be between 101 and 699.");
            }
            if (string.IsNullOrEmpty(reservation.GuestID) || reservation.GuestID.Length != 13)
            {
                return BadRequest("JMBG must be exactly 13 characters long.");
            }
            if (reservation.CheckInDate >= reservation.CheckOutDate)
            {
                return BadRequest("Check-out date must be after check-in date.");
            }
            var existingRoom = await _context.Rooms.Include(r => r.RoomType).FirstOrDefaultAsync(r => r.RoomNumber == reservation.RoomNumber);
            if (existingRoom == null)
            {
                return NotFound($"Room with number {reservation.RoomNumber} does not exist.");
            }
            if (!await _context.Guests.AnyAsync(g => g.JMBG == reservation.GuestID))
            {
                return NotFound($"Guest with ID {reservation.GuestID} does not exist.");
            }

            var existingReservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.RoomNumber == reservation.RoomNumber &&
                                          r.CheckInDate < reservation.CheckOutDate &&
                                          r.CheckOutDate > reservation.CheckInDate);

            if (existingReservation != null)
            {
                return BadRequest("The room is already reserved for the selected dates.");
            }
            decimal esTotalPrice = 0.0m;
            foreach (var esID in reservation.ExtraServiceIDs)
            {
                var es = await _context.ExtraServices.FindAsync(esID);
                if (es == null)
                {
                    return BadRequest($"Extra service with ID {esID} does not exist.");
                }
                esTotalPrice += es.Price;
            }
            
            decimal rsTotalPrice = 0.0m;
            foreach (var rsID in reservation.RoomServiceIDs)
            {
                var rs = await _context.RoomServices.FindAsync(rsID);
                if (rs == null)
                {
                    return BadRequest($"Room service with ID {rsID} does not exist.");
                }
                rsTotalPrice += rs.ItemPrice;
            }

            decimal tp = (existingRoom.RoomType.PricePerNight + esTotalPrice) * (reservation.CheckOutDate - reservation.CheckInDate).Days + rsTotalPrice; 

            if (tp < 0)
            {
                return BadRequest("Total price must be a non-negative value.");
            }

            Reservation newReservation = new Reservation
            {
                RoomNumber = reservation.RoomNumber,
                GuestID = reservation.GuestID,
                CheckInDate = reservation.CheckInDate,
                CheckOutDate = reservation.CheckOutDate,
                TotalPrice = tp, 
                RoomServices = await _context.RoomServices.Where(rs => reservation.RoomServiceIDs.Contains(rs.RoomServiceID)).ToListAsync(),
                ExtraServices = await _context.ExtraServices.Where(es => reservation.ExtraServiceIDs.Contains(es.ExtraServiceID)).ToListAsync()
            };

            await _context.Reservations.AddAsync(newReservation);
            await _context.SaveChangesAsync();
            return Ok($"Reservation created successfully for room {newReservation.RoomNumber}.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
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
            if (reservations.Count == 0)
                return NotFound("No reservations found!");
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

    // [HttpGet("GetReservationsByGuest/{jmbg}")]
    // public async Task<List<Reservation>> GetReservationsByGuest(string jmbg)
    // {
    //     try
    //     {
    //         var reservations = await _context.Reservations
    //             .Include(r => r.Room)
    //             .Include(r => r.Guest)
    //             .Where(r => r.GuestID == jmbg)
    //             .ToListAsync();
    //         // if (reservations.Count == 0)
    //         // {
    //         //     return NotFound($"No reservations with GuestID {jmbg} found.");
    //         // }
    //         return reservations;
    //     }
    //     catch (Exception ex)
    //     {
    //         List<Reservation> prazna = new List<Reservation>();
    //         return prazna;
    //         //return BadRequest(ex.Message);
    //     }
    // }

    //prethodna da bi moglo da se istestira 
    //normalna fja: 
    [HttpGet("GetReservationsByGuest/{jmbg}")]
    public async Task<IActionResult> GetReservationsByGuest(string jmbg)
    {
        try
        {
            var reservations = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.Guest)
                .Where(r => r.GuestID == jmbg)
                .ToListAsync();
            if (reservations.Count == 0)
            {
                return NotFound($"No reservations with GuestID {jmbg} found.");
            }
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("GetReservationsByRoom/{roomnumber}")]
    public async Task<IActionResult> GetReservationsByRoom(int roomnumber)
    {
        try
        {
            var reservations = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.Guest)
                .Where(r => r.RoomNumber == roomnumber)
                .ToListAsync();
            if (reservations.Count == 0)
            {
                return NotFound($"No reservations with RoomNumber {roomnumber} found.");
            }
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    [HttpGet("IsRoomAvailable/{roomNumber}/{checkIn}/{checkOut}")]
    public async Task<IActionResult> IsRoomAvailable(int roomNumber, DateTime checkIn, DateTime checkOut)
    {
        try
        {
            var overlappingReservation = await _context.Reservations
                        .Where(r => r.RoomNumber == roomNumber &&
                                    checkIn < r.CheckOutDate &&
                                    checkOut > r.CheckInDate)
                        .FirstOrDefaultAsync();

            bool isAvailable = overlappingReservation == null;

            return Ok(isAvailable);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }

    [HttpPut("UpdateReservation/{id}")]
    public async Task<IActionResult> UpdateReservation(int id, [FromBody] ReservationDTO reservation)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var existingReservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.ReservationID == id);
            if (existingReservation == null)
            {
                return NotFound($"Reservation with ID {id} not found.");
            }
            if (reservation.RoomNumber < 101 || reservation.RoomNumber > 699)
            {
                return BadRequest("Room number must be between 101 and 699.");
            }
            if (string.IsNullOrEmpty(reservation.GuestID) || reservation.GuestID.Length != 13)
            {
                return BadRequest("JMBG must be exactly 13 characters long.");
            }
            if (reservation.CheckInDate >= reservation.CheckOutDate)
            {
                return BadRequest("Check-out date must be after check-in date.");
            }
            var existingRoom = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == reservation.RoomNumber);
            if (existingRoom == null)
            {
                return NotFound($"Room with number {reservation.RoomNumber} does not exist.");
            }
            var existingGuest = await _context.Guests.FirstOrDefaultAsync(g => g.JMBG == reservation.GuestID);
            if (existingGuest == null)
            {
                return NotFound($"Guest with ID {reservation.GuestID} does not exist.");
            }


            var overlapingReservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.RoomNumber == reservation.RoomNumber &&
                                          r.CheckInDate < reservation.CheckOutDate &&
                                          r.CheckOutDate > reservation.CheckInDate);

            if (overlapingReservation != null && overlapingReservation.ReservationID != id)//da nije ta ista koju sam poslala
            {
                return BadRequest("The room is already reserved for the selected dates.");
            }

            decimal esTotalPrice = 0.0m;
            foreach (var esID in reservation.ExtraServiceIDs)
            {
                var es = await _context.ExtraServices.FindAsync(esID);
                if (es == null)
                {
                    return BadRequest($"Extra service with ID {esID} does not exist.");
                }
                esTotalPrice += es.Price;
            }
            
            decimal rsTotalPrice = 0.0m;
            foreach (var rsID in reservation.RoomServiceIDs)
            {
                var rs = await _context.RoomServices.FindAsync(rsID);
                if (rs == null)
                {
                    return BadRequest($"Room service with ID {rsID} does not exist.");
                }
                rsTotalPrice += rs.ItemPrice;
            }

            decimal tp = (existingRoom.RoomType.PricePerNight + esTotalPrice) * (reservation.CheckOutDate - reservation.CheckInDate).Days + rsTotalPrice; 

            if (tp <= 0)
            {
                return BadRequest("Total price must be a non-negative value.");
            }

            // if (existingReservation.RoomNumber != reservation.RoomNumber)
            // {
            //     existingRoom.Reservations.Remove(existingReservation);
            // }
            // if (existingReservation.GuestID != reservation.GuestID)
            // {
            //     existingGuest.Reservations.Remove(existingReservation);
            // }

            existingReservation.RoomNumber = reservation.RoomNumber;
            existingReservation.GuestID = reservation.GuestID;
            existingReservation.CheckInDate = reservation.CheckInDate;
            existingReservation.CheckOutDate = reservation.CheckOutDate;
            existingReservation.TotalPrice = tp;
            existingReservation.RoomServices = await _context.RoomServices.Where(rs => reservation.RoomServiceIDs.Contains(rs.RoomServiceID)).ToListAsync();
            existingReservation.ExtraServices = await _context.ExtraServices.Where(es => reservation.ExtraServiceIDs.Contains(es.ExtraServiceID)).ToListAsync();

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