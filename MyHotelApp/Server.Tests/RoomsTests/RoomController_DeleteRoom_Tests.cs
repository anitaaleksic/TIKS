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
     private static ReservationController _controllerReservation;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<HotelContext>()
                    .UseInMemoryDatabase(databaseName: "HotelTestDb")
                    //.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionWarning))
                    .Options;
        _context = new HotelContext(options);

        _controllerRoom = new RoomController(_context);
        _controllerReservation = new ReservationController(_context);
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

        _context.Rooms.Add(new Room
        {
            RoomNumber = 123,
            RoomTypeID = 1,
            Floor = 1
        });
        
        _context.Rooms.Add(new Room
        {
            RoomNumber = 202,
            RoomTypeID = 2,
            Floor = 2
        });
        
         var guest = new Guest
        {
            JMBG = "9473859483721",
            FullName = "Uros Miladinovic",
            PhoneNumber = "+381641336643"
        };


        _context.Guests.Add(guest);
    

        var guest1 = new Guest
        {
            JMBG = "8463859583790",
            FullName = "Anita Aleksic",
            PhoneNumber = "+381636547743"
        };


        _context.Guests.Add(guest1);
       

        var reservation = new Reservation
        {
            ReservationID = 1,
            RoomNumber = 202,
            GuestID = "9473859483721",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3),
            TotalPrice = 320.00m
        };

        _context.Reservations.Add(reservation);

        var reservation1 = new Reservation
        {
            ReservationID = 2,
            RoomNumber = 123,
            GuestID = "8463859583790",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3),
            TotalPrice = 320.00m
        };

        _context.Reservations.Add(reservation1);
        _context.SaveChanges();
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
        //await _context.SaveChangesAsync();

        
        var getRoom = await _context.Rooms.FindAsync(existingRoomNumber);
        Assert.That(getRoom, Is.Not.Null);
        
        var result = await _controllerRoom.DeleteRoom(existingRoomNumber);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult?.Value, Is.EqualTo($"Room with number {existingRoomNumber} deleted successfully."));

        var deletedRoom = await _context.Rooms.FindAsync(existingRoomNumber);
        Assert.That(deletedRoom, Is.Null);
    }
    [Test]
    public async Task DeleteRoom_WithReservations_DeletesReservations()
    {

        var roomnumber = 202;
        await _controllerRoom.DeleteRoom(roomnumber);
        _context.SaveChanges();

        var result = await _controllerReservation.GetReservationsByRoom(roomnumber);
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Has.Property("Value").EqualTo($"No reservations with RoomNumber {roomnumber} found."));
    }
    
    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}