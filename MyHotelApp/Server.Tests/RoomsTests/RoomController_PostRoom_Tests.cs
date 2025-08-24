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

public class RoomController_PostRoom_Tests
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

        _context.RoomTypes.Add(new RoomType
        {
            RoomTypeID = 1,
            Type = "Single",
            Capacity = 1,
            PricePerNight = 50m
        });
        _context.RoomTypes.Add(new RoomType
        {
            RoomTypeID = 2,
            Type = "Double",
            Capacity = 2,
            PricePerNight = 80m
        });
        _context.SaveChanges();
    }
    
    [Test]
    public async Task CreateRoom_WithRoomNumberOutOfRange_ReturnsNotFound()
    {
        var roomDto = new RoomDTO
        {
            RoomNumber = 100, // ispod minimalnog
            RoomTypeID = 1,
            Floor = 1
        };

        var result = await _controllerRoom.CreateRoom(roomDto);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var notFound = (BadRequestObjectResult)result;
        Assert.That(notFound.Value, Is.EqualTo("Room number must be between 101 and 699."));
    }

    [Test]
    public async Task CreateRoom_WithExistingRoomNumber_ReturnsNotFound()
    {
        // Prvo ubaci sobu sa brojem 123
        var existingRoom = new Room
        {
            RoomNumber = 123,
            RoomTypeID = 1,
            Floor = 1
        };
        _context.Rooms.Add(existingRoom);
        _context.SaveChanges();

        var roomDto = new RoomDTO
        {
            RoomNumber = 123, // isti broj
            RoomTypeID = 1,
            Floor = 1
        };

        var result = await _controllerRoom.CreateRoom(roomDto);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var notFound = (BadRequestObjectResult)result;
        Assert.That(notFound.Value, Is.EqualTo("Room with number 123 already exists."));
    }

    [Test]
    public async Task CreateRoom_WithInvalidFloor_ReturnsBadRequest()
    {
        var roomDto = new RoomDTO
        {
            RoomNumber = 202,
            RoomTypeID = 1,
            Floor = 7 // van validnog opsega
        };

        var result = await _controllerRoom.CreateRoom(roomDto);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badReq = (BadRequestObjectResult)result;
        Assert.That(badReq.Value, Is.EqualTo("Floor must be between 1 and 6."));
    }

    [Test]
    public async Task CreateRoom_WithNonExistingRoomType_ReturnsNotFound()
    {
        var roomDto = new RoomDTO
        {
            RoomNumber = 202,
            RoomTypeID = 999, // nepostojeći tip sobe
            Floor = 2
        };

        var result = await _controllerRoom.CreateRoom(roomDto);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFound = (NotFoundObjectResult)result;
        Assert.That(notFound.Value, Is.EqualTo("Room type with ID 999 does not exist."));
    }

    [Test]
    public async Task CreateRoom_ValidRoom_ReturnsOkAndSavesRoom()
    {
        var roomDto = new RoomDTO
        {
            RoomNumber = 305,
            RoomTypeID = 1,
            Floor = 3
        };

        var result = await _controllerRoom.CreateRoom(roomDto);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.EqualTo("Room with number 305 created successfully."));

        var savedRoom = await _context.Rooms.FindAsync(305);
        Assert.That(savedRoom, Is.Not.Null);
        Assert.That(savedRoom.RoomNumber, Is.EqualTo(305));
        Assert.That(savedRoom.Floor, Is.EqualTo(3)); 
        Assert.That(savedRoom.RoomTypeID, Is.EqualTo(1));
    }

    [Test]
    public async Task CreateRoom_FloorIgnored_ReturnsCorrectFloor()
    {
        
        var roomDto = new RoomDTO
        {
            RoomNumber = 305,
            RoomTypeID = 1,
            Floor = 5 // namerno pogrešan sprat, backend treba da ga ignoriše
        };

        var result = await _controllerRoom.CreateRoom(roomDto);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult?.Value, Is.EqualTo("Room with number 305 created successfully."));

        var savedRoom = await _context.Rooms.FindAsync(305);
        Assert.That(savedRoom, Is.Not.Null);
        Assert.That(savedRoom.Floor, Is.EqualTo(3)); // Backend je izračunao sprat na osnovu RoomNumber
    }

    [Test] //add to all Create tests
    public async Task CreateRoom_WithModelStateInvalid_ReturnsBadRequest()
    {
        // Arrange
        _controllerRoom.ModelState.AddModelError("error", "some model state error");
        var roomDTO = new RoomDTO();

        var result = await _controllerRoom.CreateRoom(roomDTO);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}