using Microsoft.EntityFrameworkCore;
using MyHotelApp.server.Models;
using MyHotelApp.Controllers;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace RoomTests;

[TestFixture]

public class RoomController_DeleteRoom_Tests
{
    private static HotelContext _context;
    private static RoomController _controllerRoom;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<HotelContext>()
                    .UseInMemoryDatabase(databaseName: "HotelTestDb")
                    //.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionWarning))
                    .Options;
        _context = new HotelContext(options);

        _controllerRoom = new RoomController(_context);
        // _controllerReservation = new ReservationController(_context);

        _context.Rooms.Add(new Room
        {
            RoomNumber = 123,
            RoomTypeID = 1,
            Floor = 1,
            IsAvailable = false
        });

        _context.Rooms.Add(new Room
        {
            RoomNumber = 202,
            RoomTypeID = 2,
            Floor = 2,
            IsAvailable = true
        });
    }

    [Test]
    public async Task DeleteRoom_WithNonExistingId_ReturnsNotFound()
    {
        int nonExistingId = 999;
        var result = await _controllerRoom.DeleteRoom(nonExistingId);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Has.Property("Value").EqualTo($"Room with number {nonExistingId} not found."));
    }

    [Test]
    public async Task DeleteRoom_WithExistingId_ReturnsOk()
    {
        int existingRoomNumber = 123;
        await _context.SaveChangesAsync();

        
        var getRoom = await _context.Rooms.FindAsync(existingRoomNumber);
        Console.WriteLine(getRoom);
        Assert.That(getRoom, Is.Not.Null);
        
        var result = await _controllerRoom.DeleteRoom(existingRoomNumber);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult?.Value, Is.EqualTo($"Room with number {existingRoomNumber} deleted successfully."));

        var deletedRoom = await _context.Rooms.FindAsync(existingRoomNumber);
        Assert.That(deletedRoom, Is.Null);
    }

    
    
    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}