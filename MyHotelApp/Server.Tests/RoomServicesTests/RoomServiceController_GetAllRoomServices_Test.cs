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

public class RoomServiceController_GetAllRoomServices_Tests
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


    }

    [Test]
    public async Task GetAllRoomServices_RoomServicesExist_ReturnsList()
    {
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
        

        var result = await _controllerRoomService.GetAllRoomServices();
        var okResult = result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);
        var roomServices = okResult.Value as List<RoomService>;
        Assert.That(roomServices, Is.Not.Null);
        Assert.That(roomServices.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetAllRoomServices_NoRoomServicesExist_ReturnsNotFound()
    {
        var result = await _controllerRoomService.GetAllRoomServices();
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Has.Property("Value").EqualTo($"No room services found."));
    }

    [Test]
    public async Task GetAllRoomServices_ReturnsOnlyRoomServiceObjects()
    {
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

        var result = await _controllerRoomService.GetAllRoomServices();
        var okResult = result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);
        var roomServices = okResult?.Value as List<RoomService>;
        Assert.That(roomServices, Is.Not.Null);

        foreach (var roomService in roomServices!)
        {
            Assert.That(roomService, Is.TypeOf<RoomService>());
        }
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

}
