using Microsoft.EntityFrameworkCore;
using MyHotelApp.server.Models;
using MyHotelApp.Controllers;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Collections.Generic;
using Microsoft.Identity.Client;

namespace GuestTests;

[TestFixture]
public class GuestController_GetGuest_Tests
{
    private static HotelContext _context;
    private static GuestController _controllerGuest;
    private static Reservation _reservation;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<HotelContext>()
                    .UseInMemoryDatabase(databaseName: "HotelTestDb")
                    .Options;

        _context = new HotelContext(options);

        _controllerGuest = new GuestController(_context);

        //seedDb       

    }

    [Test]
    public async Task GetGuest_WithNonExistingId_ReturnsNotFound()
    {
        string nonExistingId = "9999999999999";
        var result = await _controllerGuest.GetGuestByJMBG(nonExistingId);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Has.Property("Value").EqualTo($"Guest with JMBG {nonExistingId} not found."));
    }

    [Test]
    public async Task GetGuest_WithNullInput_ReturnBadRequest()
    {
        var result = await _controllerGuest.GetGuestByJMBG(null);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badReqRes = result as BadRequestObjectResult;
        Assert.That(badReqRes, Has.Property("Value").EqualTo("JMBG must be exactly 13 characters long."));
    }

    [Test]
    public async Task GetGuest_WithEmptyInput_ReturnBadRequest()
    {
        var result = await _controllerGuest.GetGuestByJMBG("");

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badReqRes = result as BadRequestObjectResult;
        Assert.That(badReqRes, Has.Property("Value").EqualTo("JMBG must be exactly 13 characters long."));
    }

    [Test]
    public async Task GetGuest_WithJMBGTooLong_ReturnBadRequest()
    {
        string jmbg = "12345678912345";
        var result = await _controllerGuest.GetGuestByJMBG(jmbg);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badReqRes = result as BadRequestObjectResult;
        Assert.That(badReqRes, Has.Property("Value").EqualTo("JMBG must be exactly 13 characters long."));
    }

    [Test]
    public async Task GetGuest_WithJMBGTooShort_ReturnBadRequest()
    {
        _context.Guests.Add(new Guest
        {
            JMBG = "1234512345123",
            FullName = "Anita Aleksic",
            PhoneNumber = "+381651234567"
        });
        _context.SaveChanges();
        string jmbg = "123456789123";
        var result = await _controllerGuest.GetGuestByJMBG(jmbg);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badReqRes = result as BadRequestObjectResult;
        Assert.That(badReqRes, Has.Property("Value").EqualTo("JMBG must be exactly 13 characters long."));
    }

    [Test]
    public async Task GetGuest_ExistingGuest_ReturnsGuest()
    {
        _context.Guests.Add(new Guest
        {
            JMBG = "1234512345123",
            FullName = "Anita Aleksic",
            PhoneNumber = "+381651234567"
        });
        _context.SaveChanges();
        var result = await _controllerGuest.GetGuestByJMBG("1234512345123");
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.InstanceOf<GuestDTO>());

        var guest = okResult.Value as GuestDTO;
        Assert.That(guest.JMBG, Is.EqualTo("1234512345123"));

    }    

    [Test]
    public async Task GetGuest_HasReservation_ReturnsGuestWithReservation()
    {
        _context.Guests.Add(new Guest
        {
            JMBG = "1234512345123",
            FullName = "Anita Aleksic",
            PhoneNumber = "+381651234567"
        });
        _context.SaveChanges();
        var room = new Room
        {
            RoomNumber = 123,
            RoomTypeID = 1,
            Floor = 1
        };

        _context.Rooms.Add(room);
        _context.SaveChanges();

        _reservation = new Reservation
        {
            ReservationID = 1,
            RoomNumber = 123,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3),
            TotalPrice = 320.00m
        };

        _context.Reservations.Add(_reservation);
        _context.SaveChanges();

        var result = await _controllerGuest.GetGuestByJMBG("1234512345123");
        var okResult = result as OkObjectResult;
        // Assert.That(okResult, Is.Not.Null);
        // Assert.That(okResult.Value, Is.InstanceOf<Guest>());

        var guest = okResult.Value as GuestDTO;
        Assert.That(guest.Reservations, Is.Not.Null);

    }

    [Test]
    public async Task GetAllGuests_GuestsExist_ReturnsList()
    {
        _context.Guests.Add(new Guest { JMBG = "1234512345123", FullName = "Anita Aleksic", PhoneNumber = "+381651234567" });
        _context.SaveChanges();

        _context.Guests.Add(new Guest { JMBG = "1234512345555", FullName = "Mila Aleksic", PhoneNumber = "+381651234333" });
        _context.SaveChanges();

        var result = await _controllerGuest.GetAllGuests();
        var okResult = result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);
        var guests = okResult.Value as List<Guest>;
        Assert.That(guests, Is.Not.Null);
        Assert.That(guests.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetAllGuests_NoGuestsExist_ReturnsNotFound()
    {
        var result = await _controllerGuest.GetAllGuests();
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Has.Property("Value").EqualTo($"No guests found."));
    }


    //no tests for get all guests

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}