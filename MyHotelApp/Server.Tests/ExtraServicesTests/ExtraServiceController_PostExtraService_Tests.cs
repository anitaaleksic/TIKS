using Microsoft.EntityFrameworkCore;
using MyHotelApp.server.Models;
using MyHotelApp.Controllers;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ExtraServiceTests;

[TestFixture]

public class ExtraServiceController_PostExtraService_Tests
{
    private static HotelContext _context;
    private static ExtraServiceController _controllerExtraService;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<HotelContext>()
                    .UseInMemoryDatabase(databaseName: "HotelTestDb")
                    //.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionWarning))
                    .Options;
        _context = new HotelContext(options);

        _controllerExtraService = new ExtraServiceController(_context);
        // _controllerReservation = new ReservationController(_context);

         _context.ExtraServices.Add(new ExtraService
        {
            ExtraServiceID = 1,
            ServiceName = "Parking Spot",
            Price = 10m,
            Description = "Reserved parking space"
        });

        _context.SaveChanges();

        _context.ExtraServices.Add(new ExtraService
        {
            ExtraServiceID = 2,
            ServiceName = "Restaurant Access",
            Price = 25m,
            Description = "Access to hotel restaurant"
        });

         _context.SaveChanges();

    }
    
    [Test]
    public async Task CreateExtraService_ValidData_ReturnsOk()
    {
        var dto = new ExtraServiceDTO
        {
            ServiceName = "Wellness Access",
            Price = 30m,
            Description = "Access to wellness center"
        };

        var result = await _controllerExtraService.CreateExtraService(dto);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var ok = result as OkObjectResult;
        Assert.That(ok?.Value, Is.EqualTo($"Extra service with name {dto.ServiceName} created successfully."));

        var created = await _context.ExtraServices.FirstOrDefaultAsync(r => r.ServiceName == dto.ServiceName);
        Assert.That(created, Is.Not.Null);
        Assert.That(created.Price, Is.EqualTo(dto.Price));
        Assert.That(created.Description, Is.EqualTo(dto.Description));
    }

    [Test]
    public async Task CreateExtraService_DuplicateName_ReturnsNotFound()
    {
        var existing = new ExtraService
        {
            ServiceName = "Wellness Access",
            Price = 30m,
            Description = "Access to wellness center"
        };
        await _context.ExtraServices.AddAsync(existing);
        await _context.SaveChangesAsync();

        var dto = new ExtraServiceDTO
        {
            ServiceName = "Wellness Access",
            Price = 30m,
            Description = "Access to wellness center"
        };

        var result = await _controllerExtraService.CreateExtraService(dto);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest?.Value, Is.EqualTo("Extra service with the name Wellness Access already exists."));
    }

    [Test]
    public async Task CreateExtraService_NullServiceName_ReturnsBadRequest()
    {
        var dto = new ExtraServiceDTO
        {
            ServiceName = "",
            Price = 30m,
            Description = "Access to wellness center"
        };

        var result = await _controllerExtraService.CreateExtraService(dto);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest?.Value, Is.EqualTo("Service name is required and cannot exceed 50 characters."));
    }

    [Test]
    public async Task CreateExtraService_TooLongServiceName_ReturnsBadRequest()
    {
        var dto = new ExtraServiceDTO
        {
            ServiceName = new string('a', 51),
            Price = 30m,
            Description = "Access to wellness center"
        };

        var result = await _controllerExtraService.CreateExtraService(dto);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest?.Value, Is.EqualTo("Service name is required and cannot exceed 50 characters."));
    }

    [Test]
    public async Task CreateExtraService_NonPositivePrice_ReturnsBadRequest()
    {
        var dto = new ExtraServiceDTO
        {
            ServiceName = "Wellness Access",
            Price = 0,
            Description = "Access to wellness center"
        };

        var result = await _controllerExtraService.CreateExtraService(dto);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest?.Value, Is.EqualTo("Price must be a positive value."));
    }

    [Test]
    public async Task PostTable_WithInvalidModelState_ReturnsBadRequest()
    {
        _controllerExtraService.ModelState.AddModelError("error", "some error");
        var ExtraServiceDTO = new ExtraServiceDTO();

        var result = await _controllerExtraService.CreateExtraService(ExtraServiceDTO);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}