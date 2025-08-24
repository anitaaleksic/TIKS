using Microsoft.EntityFrameworkCore;
using MyHotelApp.server.Models;
using MyHotelApp.Controllers;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace RoomServiceTests;

[TestFixture]

public class RoomServiceController_PostRoomService_Tests
{
    private static HotelContext _context;
    private static RoomServiceController _controllerRoomService;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<HotelContext>()
                    .UseInMemoryDatabase(databaseName: "HotelTestDb")
                    .Options;
        _context = new HotelContext(options);

        _controllerRoomService = new RoomServiceController(_context);

        _context.RoomServices.Add(new RoomService
        {
            RoomServiceID = 1,
            ItemName = "Breakfast",
            ItemPrice = 10m,
            Description = "Continental breakfast"
        });

        _context.SaveChanges();

        _context.RoomServices.Add(new RoomService
        {
            RoomServiceID = 2,
            ItemName = "Laundry",
            ItemPrice = 15m,
            Description = "Laundry service"
        });

         _context.SaveChanges();

    }
    
    [Test]
    public async Task CreateRoomService_ValidData_ReturnsOk()
    {
        var dto = new RoomServiceDTO
        {
            ItemName = "Lunch",
            ItemPrice = 15.99m,
            Description = "Continental breakfast"
        };

        var result = await _controllerRoomService.CreateRoomService(dto);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var ok = result as OkObjectResult;
        Assert.That(ok?.Value, Is.EqualTo($"Room service item with name {dto.ItemName} created successfully."));

        var created = await _context.RoomServices.FirstOrDefaultAsync(r => r.ItemName == dto.ItemName);
        Assert.That(created, Is.Not.Null);
        Assert.That(created.ItemPrice, Is.EqualTo(dto.ItemPrice));
        Assert.That(created.Description, Is.EqualTo(dto.Description));
    }

    [Test]
    public async Task CreateRoomService_DuplicateName_ReturnsNotFound()
    {
        var existing = new RoomService
        {
            ItemName = "Laundry",
            ItemPrice = 10,
            Description = "Laundry service"
        };
        await _context.RoomServices.AddAsync(existing);
        await _context.SaveChangesAsync();

        var dto = new RoomServiceDTO
        {
            ItemName = "Laundry", // duplikat imena
            ItemPrice = 12,
            Description = "Updated description"
        };

        var result = await _controllerRoomService.CreateRoomService(dto);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFound = result as NotFoundObjectResult;
        Assert.That(notFound?.Value, Is.EqualTo("Room service with the name Laundry already exists."));
    }

    [Test]
    public async Task CreateRoomService_NullItemName_ReturnsBadRequest()
    {
        var dto = new RoomServiceDTO
        {
            ItemName = "",
            ItemPrice = 20,
            Description = "Spa"
        };

        var result = await _controllerRoomService.CreateRoomService(dto);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest?.Value, Is.EqualTo("Service name is required and cannot exceed 50 characters."));
    }

    [Test]
    public async Task CreateRoomService_TooLongItemName_ReturnsBadRequest()
    {
        var dto = new RoomServiceDTO
        {
            ItemName = new string('a', 51),
            ItemPrice = 20,
            Description = "Spa"
        };

        var result = await _controllerRoomService.CreateRoomService(dto);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest?.Value, Is.EqualTo("Service name is required and cannot exceed 50 characters."));
    }

    [Test]
    public async Task CreateRoomService_NonPositivePrice_ReturnsBadRequest()
    {
        var dto = new RoomServiceDTO
        {
            ItemName = "Room cleaning",
            ItemPrice = 0,
            Description = "Should be rejected"
        };

        var result = await _controllerRoomService.CreateRoomService(dto);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest?.Value, Is.EqualTo("Price must be a positive value."));
    }

    [Test]
    public async Task PostTable_WithInvalidModelState_ReturnsBadRequest()
    {
        _controllerRoomService.ModelState.AddModelError("error", "some error");
        var roomServiceDTO = new RoomServiceDTO();

        var result = await _controllerRoomService.CreateRoomService(roomServiceDTO);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}