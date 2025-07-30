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
    public async Task<IActionResult> CreateRoom([FromBody] RoomDTO room)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingRoom = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == room.RoomNumber);
            if (room.RoomNumber < 101 || room.RoomNumber > 699)
{
                return BadRequest("Room number must be between 101 and 699.");
}
            if (existingRoom != null)
            {
                return BadRequest($"Room with number {room.RoomNumber} already exists.");
            }

            if (room.Floor < 1 || room.Floor > 6)
            {
                return BadRequest("Floor must be between 1 and 6.");
            }
            var existingRoomType = await _context.RoomTypes.FirstOrDefaultAsync(rt => rt.RoomTypeID == room.RoomTypeID);
            if (existingRoomType == null)
            {
                return BadRequest($"Room type with ID {room.RoomTypeID} does not exist.");
            }
            
            var newRoom = new Room
            {
                RoomNumber = room.RoomNumber,
                RoomTypeID = room.RoomTypeID, // Assuming RoomTypeID is the ID of the RoomType
                Floor = room.RoomNumber / 100,
                IsAvailable = room.IsAvailable
            };
 
            await _context.Rooms.AddAsync(newRoom);
            await _context.SaveChangesAsync();
            return Ok($"Room with number {newRoom.RoomNumber} created successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }
    }

    [HttpGet("GetRoom/{roomnumber}")]
    public async Task<IActionResult> GetRoom(int roomnumber)
    {
        try
        {
            var room = await _context.Rooms.Include(r=>r.RoomType).FirstOrDefaultAsync(r => r.RoomNumber == roomnumber);
            if (room == null)
            {
                return NotFound($"Room with number {roomnumber} not found.");
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
            var rooms = await _context.Rooms.Include(r=>r.RoomType).ToListAsync();
            return Ok(rooms);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("UpdateRoom/{roomNumber}")]
    public async Task<IActionResult> UpdateRoom(int roomNumber, [FromBody] RoomDTO room)
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
            // if (roomNumber != room.RoomNumber)
            // {
            //     return BadRequest("Room number in the URL does not match the room number in the body.");
            // }

            if (room.Floor < 1 || room.Floor > 6)
            {
                return BadRequest("Floor must be between 1 and 6.");
            }

            var existingRoomType = await _context.RoomTypes.FirstOrDefaultAsync(rt => rt.RoomTypeID == room.RoomTypeID);
            if (existingRoomType == null)
            {
                return BadRequest($"Room type with ID {room.RoomTypeID} does not exist.");
            }
            
            existingRoom.RoomNumber = room.RoomNumber;
            existingRoom.RoomTypeID = room.RoomTypeID;
            existingRoom.Floor = room.Floor;
            existingRoom.IsAvailable = room.IsAvailable;

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