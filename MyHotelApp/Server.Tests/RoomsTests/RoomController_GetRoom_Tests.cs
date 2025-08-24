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
    private static Reservation _reservation;

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

        var room = okResult?.Value as RoomDTO;
        Assert.That(room, Is.Not.Null);
        Assert.That(room.RoomNumber, Is.EqualTo(101));
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
        var room = okResult?.Value as RoomDTO;

        Assert.That(room, Has.Property("RoomTypeID").EqualTo(expectedRoomTypeID));
    }

    [Test]
    public async Task GetRoom_WithExistingId_ReturnsCorrectFloor()
    {
        var existingRoomNumber = 101;
        var expectedFloor = 1;

        var result = await _controllerRoom.GetRoom(existingRoomNumber);
        var okResult = result as OkObjectResult;
        var room = okResult?.Value as RoomDTO;

        Assert.That(room, Has.Property("Floor").EqualTo(expectedFloor));
    }

    [Test]
    public async Task GetRoom_WithExistingId_ReturnsCorrectRoomNumber()
    {
        var existingRoomNumber = 101;

        var result = await _controllerRoom.GetRoom(existingRoomNumber);
        var okResult = result as OkObjectResult;
        var room = okResult?.Value as RoomDTO;

        Assert.That(room, Has.Property("RoomNumber").EqualTo(existingRoomNumber));
    }

    [Test]
    public async Task GetRoomHasReservation_ReturnsRoomWithReservation()
    {
        _context.Guests.Add(new Guest
        {
            JMBG = "1234512345123",
            FullName = "Anita Aleksic",
            PhoneNumber = "+381651234567"
        });
        _context.SaveChanges();

        _reservation = new Reservation
        {
            ReservationID = 1,
            RoomNumber = 202,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3),
            TotalPrice = 320.00m
        };

        _context.Reservations.Add(_reservation);
        _context.SaveChanges();

        var result = await _controllerRoom.GetRoom(202);
        var okResult = result as OkObjectResult;

        var room = okResult.Value as RoomDTO;
        Assert.That(room.Reservations, Is.Not.Null);

    }

    

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}