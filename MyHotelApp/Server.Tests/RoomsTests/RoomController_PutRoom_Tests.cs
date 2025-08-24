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

public class RoomController_PutRoom_Tests
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
        _context.RoomTypes.AddRange(
            new RoomType
            {
                RoomTypeID = 1,
                Type = "Single",
                Capacity = 1,
                PricePerNight = 50m
            },
            new RoomType
            {
                RoomTypeID = 2,
                Type = "Double",
                Capacity = 2,
                PricePerNight = 80m
            }
        );

        _context.Rooms.Add(new Room
        {
            RoomNumber = 301,
            RoomTypeID = 1,
            Floor = 3
        });

        _context.SaveChanges();
    }
    
    [Test]
    public async Task UpdateRoom_WithNonExistingRoom_ReturnsNotFound()
    {
        var dto = new RoomDTO
        {
            RoomNumber = 404,
            RoomTypeID = 1,
            Floor = 4
        };

        var result = await _controllerRoom.UpdateRoom(404, dto);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        Assert.That(((NotFoundObjectResult)result).Value, Is.EqualTo("Room with number 404 not found."));
    }

    [Test]
    public async Task UpdateRoom_WithNegativeRoomNumber_ReturnsBadRequest()
    {
        var dto = new RoomDTO
        {
            RoomNumber = -5,
            RoomTypeID = 1,
            Floor = 2
        };

        var result = await _controllerRoom.UpdateRoom(301, dto);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        Assert.That(((BadRequestObjectResult)result).Value, Is.EqualTo("Room number must be a positive integer."));
    }

    [Test]
    public async Task UpdateRoom_WithInvalidFloor_ReturnsBadRequest()
    {
        var dto = new RoomDTO
        {
            RoomNumber = 301,
            RoomTypeID = 1,
            Floor = 7 // izvan opsega
        };

        var result = await _controllerRoom.UpdateRoom(301, dto);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        Assert.That(((BadRequestObjectResult)result).Value, Is.EqualTo("Floor must be between 1 and 6."));
    }

    [Test]
    public async Task UpdateRoom_WithNonExistingRoomType_ReturnsNotFound()
    {
        var dto = new RoomDTO
        {
            RoomNumber = 301,
            RoomTypeID = 999, // ne postoji
            Floor = 3
        };

        var result = await _controllerRoom.UpdateRoom(301, dto);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        Assert.That(((NotFoundObjectResult)result).Value, Is.EqualTo("Room type with ID 999 does not exist."));
    }

    [Test]
    public async Task UpdateRoom_ValidData_ReturnsOk()
    {
        var dto = new RoomDTO
        {
            RoomNumber = 301,
            RoomTypeID = 2, 
            Floor = 3       
        };

        var result = await _controllerRoom.UpdateRoom(301, dto);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        Assert.That(((OkObjectResult)result).Value, Is.EqualTo("Room with number 301 updated successfully."));

        var updatedRoom = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == 301);
        Assert.That(updatedRoom, Is.Not.Null);
        Assert.That(updatedRoom.RoomTypeID, Is.EqualTo(2));
        Assert.That(updatedRoom.Floor, Is.EqualTo(3));
    }

    [Test] 
    public async Task UpdateRoom_WithModelStateInvalid_ReturnsBadRequest()
    {
        _controllerRoom.ModelState.AddModelError("error", "some model state error");
        var roomDTO = new RoomDTO();
        var someValidID = 301;

        var result = await _controllerRoom.UpdateRoom(someValidID, roomDTO);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }
    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}