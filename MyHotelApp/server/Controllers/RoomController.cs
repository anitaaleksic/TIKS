using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyHotelApp.server.Models;
using System.ComponentModel.DataAnnotations;

namespace MyHotelApp.Controllers;


[ApiController]
[Route("api/[controller]")]
public class RoomController : ControllerBase
{
    private readonly HotelContext _context;

    public RoomController(HotelContext context)
    {
        _context = context;
    }

    [HttpPost("CreateRoom")]
    public async Task<IActionResult> CreateRoom([FromBody] Room room)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingRoom = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == room.RoomNumber);
            if (existingRoom != null)
            {
                return BadRequest($"Room with number {room.RoomNumber} already exists.");
            }

            if (room.Capacity < 1 || room.Capacity > 5)
            {
                return BadRequest("Capacity must be between 1 and 5.");
            }

            if (room.Floor < 1 || room.Floor > 6)
            {
                return BadRequest("Floor must be between 1 and 6.");
            }

            if (room.PricePerNight < 0)
            {
                return BadRequest("Price per night must be a positive value.");
            }
            //proveri da li je RoomType validan
            if (!Enum.IsDefined(typeof(RoomType), room.RoomType))
            {
                return BadRequest("Invalid room type.");
            }

            Room r = new Room
            {
                RoomNumber = room.RoomNumber,
                RoomType = room.RoomType,
                Capacity = room.Capacity,
                Floor = room.Floor,
                IsAvailable = room.IsAvailable,
                PricePerNight = room.PricePerNight,
                ImageUrl = room.ImageUrl
            };

            await _context.Rooms.AddAsync(r);
            await _context.SaveChangesAsync();
            return Ok($"Room with number {room.RoomNumber} created successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }
    }

    [HttpGet("GetRoom")]
    public async Task<IActionResult> GetRoom(int RoomNumber)
    {
        try
        {
            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == RoomNumber);
            if (room == null)
            {
                return NotFound($"Room with number {RoomNumber} not found.");
            }
            return Ok(room);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpGet("GetAllRooms")]
    public async Task<IActionResult> GetAllRooms()
    {
        try
        {
            if (_context.Rooms == null || !_context.Rooms.Any())
            {
                return NotFound("No rooms found.");
            }
            var rooms = await _context.Rooms.ToListAsync();
            return Ok(rooms);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("UpdateRoom/{roomNumber}")]
    public async Task<IActionResult> UpdateRoom(int roomNumber, [FromBody] Room room)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingRoom = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == roomNumber);
            if (existingRoom == null)
            {
                return NotFound($"Room with number {roomNumber} not found.");
            }
            if (room.RoomNumber <= 0)
            {
                return BadRequest("Room number must be a positive integer.");
            }
            if (roomNumber != room.RoomNumber)
            {
                return BadRequest("Room number in the URL does not match the room number in the body.");
            }
            if (room.Capacity < 1 || room.Capacity > 5)
            {
                return BadRequest("Capacity must be between 1 and 5.");
            }

            if (room.Floor < 1 || room.Floor > 6)
            {
                return BadRequest("Floor must be between 1 and 6.");
            }

            if (room.PricePerNight <= 0)
            {
                return BadRequest("Price per night must be a positive value.");
            }
            //proveri da li je RoomType validan
            if (!Enum.IsDefined(typeof(RoomType), room.RoomType))
            {
                return BadRequest("Invalid room type.");
            }

            existingRoom.RoomType = room.RoomType;
            existingRoom.Capacity = room.Capacity;
            existingRoom.Floor = room.Floor;
            existingRoom.IsAvailable = room.IsAvailable;
            existingRoom.PricePerNight = room.PricePerNight;
            existingRoom.ImageUrl = room.ImageUrl;

            await _context.SaveChangesAsync();
            return Ok($"Room with number {roomNumber} updated successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpDelete("DeleteRoom/{roomNumber}")]
    public async Task<IActionResult> DeleteRoom(int roomNumber)
    {
        try
        {
            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == roomNumber);
            if (room == null)
            {
                return NotFound($"Room with number {roomNumber} not found.");
            }

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return Ok($"Room with number {roomNumber} deleted successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

}