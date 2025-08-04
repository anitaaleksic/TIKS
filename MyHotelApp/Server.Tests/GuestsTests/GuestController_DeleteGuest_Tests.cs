using Microsoft.EntityFrameworkCore;
using MyHotelApp.server.Models;
using MyHotelApp.Controllers;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GuestTests;

[TestFixture]
public class GuestController_DeleteGuest_Tests
{
    private static HotelContext _context;
    private static GuestController _controllerGuest;
    // private static RoomController _controllerRoom;
    private static ReservationController _controllerReservation;

    [SetUp]
    public void SetUp()
    {
        //SetUp InMemory database
        var options = new DbContextOptionsBuilder<HotelContext>()
                    .UseInMemoryDatabase(databaseName: "HotelTestDb")
                    //.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionWarning))
                    .Options;
        _context = new HotelContext(options);

        _controllerGuest = new GuestController(_context);
        _controllerReservation = new ReservationController(_context);

        //seed db
        _context.Guests.Add(new Guest
        {
            JMBG = "1234512345123",
            FullName = "Anita Aleksic",
            PhoneNumber = "+381651234567"
        });
        _context.SaveChanges();

        _context.Guests.Add(new Guest
        {
            JMBG = "1234512345321",
            FullName = "Uros Miladinovic",
            PhoneNumber = "+381657654321"
        });
        _context.SaveChanges();

        _context.Guests.Add(new Guest
        {
            JMBG = "1234512345555",
            FullName = "Mila Aleksic",
            PhoneNumber = "+381651234333"
        });
        _context.SaveChanges();

        //_context

    }

    // [Test]
    // public async Task DeleteGuest_WithNonExistingId_ReturnsNotFound()
    // {
    //     string nonExistingId = "9999999999999";
    //     var result = await _controllerGuest.DeleteGuest(nonExistingId);

    //     Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    //     var notFoundResult = result as NotFoundObjectResult;
    //     Assert.That(notFoundResult, Has.Property("Value").EqualTo($"Guest with JMBG {nonExistingId} not found."));
    // }

    [Test]
    public async Task DeleteGuest_WithReservedRooms_DeletesReservations()
    {
        // var roomDto = new RoomDTO
        // {
        //     RoomNumber = 123,
        //     RoomTypeID = 1,
        //     Floor = 1,
        //     IsAvailable = false
        // };

        var reservationDto = new ReservationDTO
        {
            ReservationID = 1,
            RoomNumber = 123,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3),
            TotalPrice = 320.00m
        };

        var reservationDto1 = new ReservationDTO
        {
            ReservationID = 2,
            RoomNumber = 124,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3),
            TotalPrice = 320.00m
        };

        var jmbg = "1234512345123";

        var guestReservations = await _controllerReservation.GetReservationsByGuest(jmbg);
        Assert.That(guestReservations.Count, Is.EqualTo(2));
        


        // var result = await _controllerGuest.DeleteGuest(jmbg);
        // var guestReservations = await _controllerReservation.GetReservationsByGuest(jmbg);

        // Assert.That(guestReservations, Is.InstanceOf<NotFoundObjectResult>());
        // var notFoundResult = guestReservations as NotFoundObjectResult;
        // Assert.That(notFoundResult, Has.Property("Value").EqualTo($"No reservations with GuestID {jmbg} found."));

    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

}

