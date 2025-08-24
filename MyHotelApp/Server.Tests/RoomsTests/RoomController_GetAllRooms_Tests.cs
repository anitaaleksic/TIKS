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

public class RoomController_GetAllRooms_Tests
{
    private static HotelContext _context;
    private static RoomController _controllerRoom;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<HotelContext>()
                    .UseInMemoryDatabase(databaseName: "HotelTestDb")
                    .Options;
        _context = new HotelContext(options);

        _controllerRoom = new RoomController(_context);
    }

    [Test]
    public async Task GetAllRooms_RoomsExist_ReturnsList()
    {
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

        var result = await _controllerRoom.GetAllRooms();
        var okResult = result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);
        var rooms = okResult.Value as List<Room>;
        Assert.That(rooms, Is.Not.Null);
        Assert.That(rooms.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetAllRooms_NoRoomsExist_ReturnsNotFound()
    {
        var result = await _controllerRoom.GetAllRooms();
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Has.Property("Value").EqualTo($"No rooms found."));
    }

    [Test]
    public async Task GetAllRooms_ReturnsOnlyRoomObjects()
    {
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

        var result = await _controllerRoom.GetAllRooms();
        var okResult = result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);
        var rooms = okResult?.Value as List<Room>;
        Assert.That(rooms, Is.Not.Null);

        foreach (var room in rooms!)
        {
            Assert.That(room, Is.TypeOf<Room>());
        }
    }

    
    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}