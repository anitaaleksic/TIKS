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
public class GuestController_DeleteGuest_Tests
{
    private static HotelContext _context;
    private static GuestController _controllerGuest;
    private static ReservationController _controllerReservation;

    private Reservation _reservation;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<HotelContext>()
                    .UseInMemoryDatabase(databaseName: "HotelTestDb")
                    .Options;
        _context = new HotelContext(options);

        _controllerGuest = new GuestController(_context);
        _controllerReservation = new ReservationController(_context);

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

        // _context.Guests.Add(new Guest
        // {
        //     JMBG = "1234512345321",
        //     FullName = "Uros Miladinovic",
        //     PhoneNumber = "+381657654321"
        // });
        // _context.SaveChanges();

        // _context.Guests.Add(new Guest
        // {
        //     JMBG = "1234512345555",
        //     FullName = "Mila Aleksic",
        //     PhoneNumber = "+381651234333"
        // });
        // _context.SaveChanges();

        //var room1 = new Room
        // {
        //     RoomNumber = 124,
        //     RoomTypeID = 1,
        //     Floor = 1
        // };


        // _context.Rooms.Add(room1);
        // _context.SaveChanges();

        // var reservation1 = new Reservation
        // {
        //     ReservationID = 2,
        //     RoomNumber = 124,
        //     GuestID = "1234512345123",
        //     CheckInDate = new DateTime(2025, 9, 1),
        //     CheckOutDate = new DateTime(2025, 9, 3),
        //     TotalPrice = 320.00m
        // };

        // _context.Reservations.Add(reservation1);
        // _context.SaveChanges();


    }

    [Test]
    public async Task DeleteGuest_WithNonExistingId_ReturnsNotFound()
    {
        string nonExistingId = "9999999999999";
        var result = await _controllerGuest.DeleteGuest(nonExistingId);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Has.Property("Value").EqualTo($"Guest with JMBG {nonExistingId} not found."));
    }

    [Test]
    public async Task DeleteGuest_WithReservedRoom_DeletesReservation()
    {        
        var jmbg = "1234512345123";

        await _controllerGuest.DeleteGuest(jmbg);
        _context.SaveChanges();

        var result = await _controllerReservation.GetReservationsByGuest(jmbg);
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Has.Property("Value").EqualTo($"No reservations with GuestID {jmbg} found."));

        //provere
        // var existingGuest = await _controllerGuest.GetGuestByJMBG(jmbg);
        // Assert.That(existingGuest, Is.InstanceOf<OkObjectResult>());

        //Console.WriteLine("Upomocyyy");

        // var okResult = result as OkObjectResult;
        // var guestReservations = okResult.Value as List<Reservation>;
        // Assert.That(guestReservations.Count, Is.EqualTo(0));

        // Assert.That(guestReservations2.Count, Is.EqualTo(0));
        /*

                //Assert.That(result, Is.InstanceOf<OkObjectResult>());

                //Assert.That(guestReservations.Count, Is.EqualTo(2));

        */


        // var guestReservations = await _controllerReservation.GetReservationsByGuest(jmbg);
        // Assert.That(guestReservations.Count, Is.EqualTo(2));

        //Assert.That(guestReservations, Has.Length(2));
        //CollectionAssert.IsNotEmpty(guestReservations);

        //Assert.That(guestReservations, Is.Not.Empty);

        // var result = await _controllerGuest.DeleteGuest(jmbg);
        // var guestReservations = await _controllerReservation.GetReservationsByGuest(jmbg);

        // Assert.That(guestReservations, Is.InstanceOf<NotFoundObjectResult>());
        // var notFoundResult = guestReservations as NotFoundObjectResult;
        // Assert.That(notFoundResult, Has.Property("Value").EqualTo($"No reservations with GuestID {jmbg} found."));

    }

    [Test]
    public async Task DeleteGuest_WithReservedRooms_RoomsBecomeAvailable()
    {
        var jmbg = "1234512345123";

        await _controllerGuest.DeleteGuest(jmbg);
        _context.SaveChanges();

        var roomAvailability = await _controllerReservation.IsRoomAvailable(_reservation.RoomNumber, _reservation.CheckInDate, _reservation.CheckOutDate);
        var okAvailability = roomAvailability as OkObjectResult;

        Assert.That(okAvailability.Value, Is.True);

        //CHECKING 
        // var existingGuest = await _controllerGuest.GetGuestByJMBG(jmbg);
        // Assert.That(existingGuest, Is.InstanceOf<OkObjectResult>());

        // var result = await _controllerReservation.GetReservationsByGuest(jmbg);
        // var okResult = result as OkObjectResult;
        // var guestReservations = okResult.Value as List<Reservation>;
        // Assert.That(guestReservations.Count, Is.EqualTo(2));

        // foreach (var res in guestReservations)
        // {
        //     Console.WriteLine($"reservation: {res.RoomNumber}, {res.CheckInDate}, {res.CheckOutDate}");
        //     var resultAvailable = await _controllerReservation.IsRoomAvailable(res.RoomNumber, res.CheckInDate, res.CheckOutDate);
        //     var okResultAvailable = resultAvailable as OkObjectResult;
        //     Console.WriteLine($"status: {okResultAvailable.Value}");
        // }

        // await _controllerGuest.DeleteGuest(jmbg);
        // _context.SaveChanges();

        // foreach (var res in guestReservations)
        // {
        //     Console.WriteLine($"reservation: {res.RoomNumber}, {res.CheckInDate}, {res.CheckOutDate}");
        //     var resultAvailable = await _controllerReservation.IsRoomAvailable(res.RoomNumber, res.CheckInDate, res.CheckOutDate);
        //     var okResultAvailable = resultAvailable as OkObjectResult;
        //     Console.WriteLine($"status: {okResultAvailable.Value}");
        // }

    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

}

