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

public class RoomServiceController_DeleteRoomService_Tests
{
    private static HotelContext _context;
    private static RoomServiceController _controllerRoomService;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<HotelContext>()
                    .UseInMemoryDatabase(databaseName: "HotelTestDb")
                    //.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionWarning))
                    .Options;
        _context = new HotelContext(options);

        _controllerRoomService = new RoomServiceController(_context);
        // _controllerReservation = new ReservationController(_context);

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
    public async Task DeleteRoomService_WithValidId_ReturnsOkAndDeletesService()
    {
        var service = new RoomService
        {
            ItemName = "Spa",
            ItemPrice = 50,
            Description = "Spa service"
        };
        await _context.RoomServices.AddAsync(service);
        await _context.SaveChangesAsync();

        var result = await _controllerRoomService.DeleteRoomService(service.RoomServiceID);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var ok = result as OkObjectResult;
        Assert.That(ok?.Value, Is.EqualTo($"Room service with ID {service.RoomServiceID} deleted successfully."));

        var deleted = await _context.RoomServices.FindAsync(service.RoomServiceID);
        Assert.That(deleted, Is.Null);
    }

    [Test]
    public async Task DeleteRoomService_WithNonExistingId_ReturnsNotFound()
    {
        var nonExistingId = 999;

        var result = await _controllerRoomService.DeleteRoomService(nonExistingId);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFound = result as NotFoundObjectResult;
        Assert.That(notFound?.Value, Is.EqualTo($"Room service with ID {nonExistingId} not found."));
    }

    [Test]
    public async Task DeleteRoomService_Twice_SecondTimeReturnsNotFound()
    {
        var service = new RoomService
        {
            ItemName = "Gym",
            ItemPrice = 20,
            Description = "Access to gym"
        };
        await _context.RoomServices.AddAsync(service);
        await _context.SaveChangesAsync();

        var firstDelete = await _controllerRoomService.DeleteRoomService(service.RoomServiceID);
        Assert.That(firstDelete, Is.InstanceOf<OkObjectResult>());

        var secondDelete = await _controllerRoomService.DeleteRoomService(service.RoomServiceID);
        Assert.That(secondDelete, Is.InstanceOf<NotFoundObjectResult>());
        var notFound = secondDelete as NotFoundObjectResult;
        Assert.That(notFound?.Value, Is.EqualTo($"Room service with ID {service.RoomServiceID} not found."));
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}