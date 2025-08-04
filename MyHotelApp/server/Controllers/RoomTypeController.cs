using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyHotelApp.server.Models;
using System.ComponentModel.DataAnnotations;

namespace MyHotelApp.Controllers;


[ApiController]
[Route("api/[controller]")]
public class RoomTypeController : ControllerBase
{
    private readonly HotelContext _context;

    public RoomTypeController(HotelContext context)
    {
        _context = context;
    }

    [HttpPost("CreateRoomType")]
    public async Task<IActionResult> CreateRoomType([FromBody] RoomTypeDTO roomType)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingRoomType = await _context.RoomTypes.FirstOrDefaultAsync(r => r.Type == roomType.Type);
            if (existingRoomType != null)
            {
                return BadRequest($"Room type {roomType.Type} already exists.");
            }

            if (roomType.Capacity <= 0)
            {
                return BadRequest("Capacity must be a positive number.");
            }
            if (roomType.PricePerNight <= 0)
            {
                return BadRequest("Price per night must be a positive number.");
            }

            var newRoomType = new RoomType
            {
                Type = roomType.Type,
                Capacity = roomType.Capacity,
                PricePerNight = roomType.PricePerNight
            };

            await _context.RoomTypes.AddAsync(newRoomType);
            await _context.SaveChangesAsync();


            return Ok($"RoomType {roomType.Type} created successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }
    }

    [HttpGet("GetRoomType/{type}")]
    public async Task<IActionResult> GetRoomType(string type)
    {
        try
        {
            var roomType = await _context.RoomTypes.FirstOrDefaultAsync(rt => rt.Type == type);
            if (roomType == null)
            {
                return NotFound($"Room type {type} not found.");
            }
            return Ok(roomType);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("GetRoomTypeById/{id}")]
    public async Task<IActionResult> GetRoomTypeById(int id)
    {
        try
        {
            var roomType = await _context.RoomTypes.FirstOrDefaultAsync(rt => rt.RoomTypeID == id);
            if (roomType == null)
            {
                return NotFound($"Room type with id {id} not found.");
            }
            return Ok(roomType);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("GetAllRoomTypes")]
    public async Task<IActionResult> GetAllRoomTypes()
    {
        try
        {
            if (_context.RoomTypes == null || !_context.RoomTypes.Any())
            {
                return NotFound("No room types found.");
            }
            var roomTypes = await _context.RoomTypes.ToListAsync();
            return Ok(roomTypes);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // [HttpPut("UpdateRoom/{roomNumber}")]
    // public async Task<IActionResult> UpdateRoom(int roomNumber, [FromBody] RoomDTO room)
    // {
    //     try
    //     {
    //         if (!ModelState.IsValid)
    //         {
    //             return BadRequest(ModelState);
    //         }

    //         var existingRoom = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == roomNumber);
    //         if (existingRoom == null)
    //         {
    //             return NotFound($"Room with number {roomNumber} not found.");
    //         }
    //         if (room.RoomNumber <= 0)
    //         {
    //             return BadRequest("Room number must be a positive integer.");
    //         }
    //         // if (roomNumber != room.RoomNumber)
    //         // {
    //         //     return BadRequest("Room number in the URL does not match the room number in the body.");
    //         // }

    //         if (room.Floor < 1 || room.Floor > 6)
    //         {
    //             return BadRequest("Floor must be between 1 and 6.");
    //         }

    //         var existingRoomType = await _context.RoomTypes.FirstOrDefaultAsync(rt => rt.RoomTypeID == room.RoomTypeID);
    //         if (existingRoomType == null)
    //         {
    //             return BadRequest($"Room type with ID {room.RoomTypeID} does not exist.");
    //         }

    //         existingRoom.RoomNumber = room.RoomNumber;
    //         existingRoom.RoomTypeID = room.RoomTypeID;
    //         existingRoom.Floor = room.Floor;
    //         existingRoom.IsAvailable = room.IsAvailable;

    //         await _context.SaveChangesAsync();
    //         return Ok($"Room with number {roomNumber} updated successfully.");
    //     }
    //     catch (Exception ex)
    //     {
    //         return BadRequest(ex.Message);
    //     }
    // }

    [HttpDelete("DeleteRoomTypeById/{id}")]
    public async Task<IActionResult> DeleteRoomTypeById(int id)
    {
        try
        {
            var roomType = await _context.RoomTypes.FirstOrDefaultAsync(rt => rt.RoomTypeID == id);
            if (roomType == null)
            {
                return NotFound($"Room type with id {id} not found.");
            }

            _context.RoomTypes.Remove(roomType);
            await _context.SaveChangesAsync();
            return Ok($"Room type with id {id} deleted successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("DeleteRoomType/{type}")]
    public async Task<IActionResult> DeleteRoomType(string type)
    {
        try
        {
            var roomType = await _context.RoomTypes.FirstOrDefaultAsync(rt => rt.Type == type);
            if (roomType == null)
            {
                return NotFound($"Room type {type} not found.");
            }

            _context.RoomTypes.Remove(roomType);
            await _context.SaveChangesAsync();
            return Ok($"Room type {type} deleted successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}