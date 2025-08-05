using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyHotelApp.server.Models;
using System.ComponentModel.DataAnnotations;

namespace MyHotelApp.Controllers;


[ApiController]
[Route("api/[controller]")]
public class RoomServiceController : ControllerBase
{
    private readonly HotelContext _context;

    public RoomServiceController(HotelContext context)
    {
        _context = context;
    }

    [HttpPost("CreateRoomService")]
    public async Task<IActionResult> CreateRoomService([FromBody] RoomServiceDTO roomService)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var rs = await _context.RoomServices.FirstOrDefaultAsync(rs => rs.ItemName == roomService.ItemName);
            if (rs != null)
            {
                return NotFound($"Room service with the name {roomService.ItemName} already exists.");
            }
            if (string.IsNullOrEmpty(roomService.ItemName) || roomService.ItemName.Length > 50)
            {
                return BadRequest("Service name is required and cannot exceed 50 characters.");
            }

            if (roomService.ItemPrice <= 0)
            {
                return BadRequest("Price must be a positive value.");
            }

            var newRoomService = new RoomService
            {
                ItemName = roomService.ItemName,
                ItemPrice = roomService.ItemPrice,
                Description = roomService.Description
            };

            await _context.RoomServices.AddAsync(newRoomService);
            await _context.SaveChangesAsync();
            return Ok($"Room service item with name {newRoomService.ItemName} created successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("GetAllRoomServices")]
    public async Task<IActionResult> GetAllRoomServices()
    {
        try
        {
            if (_context.RoomServices == null || !_context.RoomServices.Any())
            {
                return NotFound("No room services found.");
            }
            var roomServices = await _context.RoomServices.ToListAsync();
            return Ok(roomServices);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("GetRoomServiceById/{id}")]
    public async Task<IActionResult> GetRoomServiceById(int id)
    {
        try
        {
            var roomService = await _context.RoomServices.FirstOrDefaultAsync(rs => rs.RoomServiceID == id);
            if (roomService == null)
            {
                return NotFound($"Room service with ID {id} not found.");
            }
            return Ok(roomService);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpGet("GetRoomServiceByName/{serviceName}")]
    public async Task<IActionResult> GetRoomServiceByName(string serviceName)
    {
        try
        {
            var roomService = await _context.RoomServices.FirstOrDefaultAsync(rs => rs.ItemName == serviceName);
            if (roomService == null)
            {
                return NotFound($"Room service with name {serviceName} not found.");
            }
            return Ok(roomService);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("GetReservationsByRoomServiceId/{roomServiceId}")]
    public async Task<IActionResult> GetReservationsByRoomServiceId(int roomServiceId)
    {
        try
        {
            var roomService = await _context.RoomServices
                .Include(rs => rs.AddedToReservations)  // Učitaj povezane rezervacije
                .FirstOrDefaultAsync(rs => rs.RoomServiceID == roomServiceId);

            if (roomService == null)
            {
                return NotFound($"Room service with ID {roomServiceId} not found.");
            }

            var reservations = roomService.AddedToReservations;

            // Možeš vratiti direktno reservations ili mapirati u DTO ako želiš:
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpPut("UpdateRoomService/{id}")]
    public async Task<IActionResult> UpdateRoomService(int id, [FromBody] RoomServiceDTO roomService)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var rs = await _context.RoomServices.FirstOrDefaultAsync(rs => rs.RoomServiceID == id);
            if (rs == null)
            {
                return NotFound($"Room service with ID {id} not found.");
            }

            if (string.IsNullOrEmpty(roomService.ItemName) || roomService.ItemName.Length > 100)
            {
                return BadRequest("Service name is required and cannot exceed 100 characters.");
            }
            if(await _context.RoomServices.AnyAsync(rs => rs.ItemName == roomService.ItemName && rs.RoomServiceID != id))
            {
                return BadRequest($"Room service with the name {roomService.ItemName} already exists.");
            }
    
            if (roomService.ItemPrice <= 0)
            {
                return BadRequest("Price must be a positive value.");
            }

            rs.ItemName = roomService.ItemName;
            rs.ItemPrice = roomService.ItemPrice;
            rs.Description = roomService.Description;

            await _context.SaveChangesAsync();
            return Ok($"Room service with ID {id} updated successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("UpdateRoomServiceByName/{serviceName}")]
    public async Task<IActionResult> UpdateRoomServiceByName(string serviceName, [FromBody] RoomServiceDTO roomService)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var rs = await _context.RoomServices.FirstOrDefaultAsync(rs => rs.ItemName == serviceName);
            if (rs == null)
            {
                return NotFound($"Room service with name {serviceName} not found.");
            }

            if (string.IsNullOrEmpty(roomService.ItemName) || roomService.ItemName.Length > 50)
            {
                return BadRequest("Service name is required and cannot exceed 50 characters.");
            }

            if(await _context.RoomServices.AnyAsync(rserv => rserv.ItemName == roomService.ItemName && rserv.RoomServiceID != rs.RoomServiceID))
            {
                return BadRequest($"Room service with the name {roomService.ItemName} already exists.");
            }

            if (roomService.ItemPrice <= 0)
            {
                return BadRequest("Price must be a positive value.");
            }

            rs.ItemName = roomService.ItemName;
            rs.ItemPrice = roomService.ItemPrice;
            rs.Description = roomService.Description;

            await _context.SaveChangesAsync();
            return Ok($"Room service with name {serviceName} updated successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("DeleteRoomService/{id}")]
    public async Task<IActionResult> DeleteRoomService(int id)
    {
        try
        {
            var roomService = await _context.RoomServices.FirstOrDefaultAsync(rs => rs.RoomServiceID == id);
            if (roomService == null)
            {
                return NotFound($"Room service with ID {id} not found.");
            }

            _context.RoomServices.Remove(roomService);
            await _context.SaveChangesAsync();
            return Ok($"Room service with ID {id} deleted successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("DeleteRoomServiceByName/{serviceName}")]
    public async Task<IActionResult> DeleteRoomServiceByName(string serviceName)
    {
        try
        {
            var roomService = await _context.RoomServices.FirstOrDefaultAsync(rs => rs.ItemName == serviceName);
            if (roomService == null)
            {
                return NotFound($"Room service with name {serviceName} not found.");
            }

            _context.RoomServices.Remove(roomService);
            await _context.SaveChangesAsync();
            return Ok($"Room service with name {serviceName} deleted successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}