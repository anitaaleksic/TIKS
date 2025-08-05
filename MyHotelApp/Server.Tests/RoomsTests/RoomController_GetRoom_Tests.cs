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

public class RoomController_GetRoom_Tests
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

        _context.RoomTypes.AddRange(
            new RoomType
            {
                RoomTypeID = 1,
                Type = "Single",
                Capacity = 1,
                PricePerNight = 50.00m
            },
            new RoomType
            {
                RoomTypeID = 2,
                Type = "Double",
                Capacity = 2,
                PricePerNight = 80.00m
            }
        );
        _context.SaveChanges();

        _context.Rooms.AddRange(
            new Room
            {
                RoomNumber = 101,
                RoomTypeID = 1,
                Floor = 1,
            },
            new Room
            {
                RoomNumber = 202,
                RoomTypeID = 2,
                Floor = 2,
            }
        );
        _context.SaveChanges();
    }
    
    [Test]
    public async Task GetRoom_WithExistingRoomNumber_ReturnsRoomWithRoomType()
    {
        var result = await _controllerRoom.GetRoom(101);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());

        var okResult = result as OkObjectResult;
        Assert.That(okResult?.Value, Is.Not.Null);

        var room = okResult?.Value as Room;
        Assert.That(room, Is.Not.Null);
        Assert.That(room.RoomNumber, Is.EqualTo(101));
        Assert.That(room.RoomType, Is.Not.Null);
        Assert.That(room.RoomType.Type, Is.EqualTo("Single"));
        Assert.That(room.RoomType.PricePerNight, Is.EqualTo(50.00m));
    }

    [Test]
    public async Task GetRoom_WithNonExistingRoomNumber_ReturnsNotFound()
    {
        var result = await _controllerRoom.GetRoom(999);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult?.Value, Is.EqualTo("Room with number 999 not found."));
    }

    [Test]
    public async Task GetRoom_WithExistingId_ReturnsCorrectRoomTypeID()
    {
        var existingRoomNumber = 101;
        var expectedRoomTypeID = 1;

        var result = await _controllerRoom.GetRoom(existingRoomNumber);
        var okResult = result as OkObjectResult;
        var room = okResult?.Value as Room;

        Assert.That(room, Has.Property("RoomTypeID").EqualTo(expectedRoomTypeID));
    }

    [Test]
    public async Task GetRoom_WithExistingId_ReturnsCorrectFloor()
    {
        var existingRoomNumber = 101;
        var expectedFloor = 1;

        var result = await _controllerRoom.GetRoom(existingRoomNumber);
        var okResult = result as OkObjectResult;
        var room = okResult?.Value as Room;

        Assert.That(room, Has.Property("Floor").EqualTo(expectedFloor));
    }

    [Test]
    public async Task GetRoom_WithExistingId_ReturnsCorrectRoomNumber()
    {
        var existingRoomNumber = 101;

        var result = await _controllerRoom.GetRoom(existingRoomNumber);
        var okResult = result as OkObjectResult;
        var room = okResult?.Value as Room;

        Assert.That(room, Has.Property("RoomNumber").EqualTo(existingRoomNumber));
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}