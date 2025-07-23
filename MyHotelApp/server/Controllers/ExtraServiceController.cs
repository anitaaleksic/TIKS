using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyHotelApp.server.Models;
using System.ComponentModel.DataAnnotations;

namespace MyHotelApp.Controllers;


[ApiController]
[Route("api/[controller]")]
public class ExtraServiceController : ControllerBase
{
    private readonly HotelContext _context;

    public ExtraServiceController(HotelContext context)
    {
        _context = context;
    }

    [HttpPost("CreateExtraService")]
    public async Task<IActionResult> CreateExtraService([FromBody] ExtraService extraService)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var es = await _context.ExtraServices.FirstOrDefaultAsync(es => es.ServiceName == extraService.ServiceName);
            if (es != null)
            {
                return BadRequest($"Extra service with the name {extraService.ServiceName} already exists.");
            }
            if (string.IsNullOrEmpty(extraService.ServiceName) || extraService.ServiceName.Length > 100)
            {
                return BadRequest("Service name is required and cannot exceed 100 characters.");
            }

            if (extraService.Price <= 0)
            {
                return BadRequest("Price must be a positive value.");
            }

            ExtraService newExtraService = new ExtraService
            {
                ServiceName = extraService.ServiceName,
                Price = extraService.Price,
                Description = extraService.Description
            };

            await _context.ExtraServices.AddAsync(newExtraService);
            await _context.SaveChangesAsync();
            return Ok($"Extra service with name {newExtraService.ServiceName} created successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("GetAllExtraServices")]
    public async Task<IActionResult> GetAllExtraServices()
    {
        try
        {
            if (_context.ExtraServices == null || !_context.ExtraServices.Any())
            {
                return BadRequest("No extra services found.");
            }
            var extraServices = await _context.ExtraServices.ToListAsync();
            return Ok(extraServices);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("GetExtraServiceById/{id}")]
    public async Task<IActionResult> GetExtraServiceById(int id)
    {
        try
        {
            var extraService = await _context.ExtraServices.FirstOrDefaultAsync(es => es.ExtraServiceID == id);
            if (extraService == null)
            {
                return NotFound($"Extra service with ID {id} not found.");
            }
            return Ok(extraService);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpGet("GetExtraServiceByName/{serviceName}")]
    public async Task<IActionResult> GetExtraServiceByName(string serviceName)
    {
        try
        {
            var extraService = await _context.ExtraServices.FirstOrDefaultAsync(es => es.ServiceName == serviceName);
            if (extraService == null)
            {
                return NotFound($"Extra service with name {serviceName} not found.");
            }
            return Ok(extraService);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpPut("UpdateExtraService/{id}")]
    public async Task<IActionResult> UpdateExtraService(int id, [FromBody] ExtraService extraService)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var es = await _context.ExtraServices.FirstOrDefaultAsync(es => es.ExtraServiceID == id);
            if (es == null)
            {
                return NotFound($"Extra service with ID {id} not found.");
            }

            if (string.IsNullOrEmpty(extraService.ServiceName) || extraService.ServiceName.Length > 100)
            {
                return BadRequest("Service name is required and cannot exceed 100 characters.");
            }
            if(await _context.ExtraServices.AnyAsync(es => es.ServiceName == extraService.ServiceName && es.ExtraServiceID != id))
            {
                return BadRequest($"Extra service with the name {extraService.ServiceName} already exists.");
            }

            if (extraService.Price <= 0)
            {
                return BadRequest("Price must be a positive value.");
            }

            es.ServiceName = extraService.ServiceName;
            es.Price = extraService.Price;
            es.Description = extraService.Description;

            await _context.SaveChangesAsync();
            return Ok($"Extra service with ID {id} updated successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpPut("UpdateExtraServiceByName/{serviceName}")]
    public async Task<IActionResult> UpdateExtraServiceByName(string serviceName, [FromBody] ExtraService extraService)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var es = await _context.ExtraServices.FirstOrDefaultAsync(es => es.ServiceName == serviceName);
            if (es == null)
            {
                return NotFound($"Extra service with name {serviceName} not found.");
            }

            if (string.IsNullOrEmpty(extraService.ServiceName) || extraService.ServiceName.Length > 100)
            {
                return BadRequest("Service name is required and cannot exceed 100 characters.");
            }
            if(await _context.ExtraServices.AnyAsync(eserv => eserv.ServiceName == extraService.ServiceName && eserv.ExtraServiceID != es.ExtraServiceID))
            {
                return BadRequest($"Extra service with the name {extraService.ServiceName} already exists.");
            }

            if (extraService.Price <= 0)
            {
                return BadRequest("Price must be a positive value.");
            }

            es.ServiceName = extraService.ServiceName;
            es.Price = extraService.Price;
            es.Description = extraService.Description;

            await _context.SaveChangesAsync();
            return Ok($"Extra service with name {serviceName} updated successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("DeleteExtraService/{id}")]
    public async Task<IActionResult> DeleteExtraService(int id)
    {
        try
        {
            var extraService = await _context.ExtraServices.FirstOrDefaultAsync(es => es.ExtraServiceID == id);
            if (extraService == null)
            {
                return BadRequest($"Extra service with ID {id} not found.");
            }

            _context.ExtraServices.Remove(extraService);
            await _context.SaveChangesAsync();
            return Ok($"Extra service with ID {id} deleted successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("DeleteExtraServiceByName/{serviceName}")]
    public async Task<IActionResult> DeleteExtraServiceByName(string serviceName)
    {
        try
        {
            var extraService = await _context.ExtraServices.FirstOrDefaultAsync(es => es.ServiceName == serviceName);
            if (extraService == null)
            {
                return BadRequest($"Extra service with name {serviceName} not found.");
            }

            _context.ExtraServices.Remove(extraService);
            await _context.SaveChangesAsync();
            return Ok($"Extra service with name {serviceName} deleted successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

}