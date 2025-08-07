using Microsoft.EntityFrameworkCore;
using MyHotelApp.server.Models;
using MyHotelApp.Controllers;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Collections.Generic;
using Microsoft.Identity.Client;

namespace ReservationTests;

[TestFixture]
public class ReservationController_GetReservation_Tests
{
    private static HotelContext _context;
    private static ReservationController _controllerReservation;
    private static RoomServiceController _controllerRoomService;
    private static ExtraServiceController _controllerExtraService;
    private static RoomController _controllerRoom;
    private static GuestController _controllerGuest;
    //private Reservation _reservation;
    //private Room _room;
    private RoomService _roomService;
    private ExtraService _extraService;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<HotelContext>()
                    .UseInMemoryDatabase(databaseName: "HotelTestDb")
                    .Options;
        _context = new HotelContext(options);
        _controllerReservation = new ReservationController(_context);
        _controllerRoomService = new RoomServiceController(_context);
        _controllerExtraService = new ExtraServiceController(_context);
        _controllerGuest = new GuestController(_context);
        _controllerRoom = new RoomController(_context);

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
            JMBG = "1234567890123",
            FullName = "Mila Aleksic",
            PhoneNumber = "+381651234567"
        });
        _context.SaveChanges();

        _context.RoomTypes.Add(new RoomType
        {
            RoomTypeID = 1,
            Type = "single",
            Capacity = 1,
            PricePerNight = 120.00m
        });
        _context.SaveChanges();

        Room room = new Room
        {
            RoomNumber = 123,
            RoomTypeID = 1,
            Floor = 1
        };

        _context.Rooms.Add(room);
        _context.SaveChanges();

        Room room1 = new Room
        {
            RoomNumber = 699,
            RoomTypeID = 1,
            Floor = 6
        };

        _context.Rooms.Add(room1);
        _context.SaveChanges();

        Room room2 = new Room
        {
            RoomNumber = 500,
            RoomTypeID = 1,
            Floor = 5
        };

        _context.Rooms.Add(room2);
        _context.SaveChanges();

        _roomService = new RoomService
        {
            RoomServiceID = 1,
            ItemName = "Breakfast",
            ItemPrice = 10m,
            Description = "Continental breakfast"
        };
        _context.RoomServices.Add(_roomService);

        _context.SaveChanges();

        _context.RoomServices.Add(new RoomService
        {
            RoomServiceID = 2,
            ItemName = "Laundry",
            ItemPrice = 15m,
            Description = "Laundry service"
        });

        _context.SaveChanges();

        _extraService = new ExtraService
        {
            ExtraServiceID = 1,
            ServiceName = "Parking Spot",
            Price = 20m,
            Description = "Reserved parking space"
        };

        _context.ExtraServices.Add(_extraService);

        _context.SaveChanges();

        ExtraService extraService2 = new ExtraService
        {
            ExtraServiceID = 2,
            ServiceName = "Restaurant Access",
            Price = 25m,
            Description = "Access to hotel restaurant"
        };

        _context.ExtraServices.Add(extraService2);

        _context.SaveChanges();
    }

    [Test]
    public async Task GetReservationById_ValidId_ReturnsReservationWithRoomAndGuest()
    {
        Reservation reservation = new Reservation
        {
            ReservationID = 1,
            RoomNumber = 699,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3)
        };
        _context.Reservations.Add(reservation);
        _context.SaveChanges();

        var result = await _controllerReservation.GetReservationById(1);
        var okResult = result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);
        var returnedReservation = okResult.Value as Reservation;

        Assert.That(returnedReservation, Is.Not.Null);
        Assert.That(returnedReservation.Room, Is.Not.Null);
        Assert.That(returnedReservation.Guest, Is.Not.Null);
        Assert.That(returnedReservation.Room.RoomNumber, Is.EqualTo(699));
        Assert.That(returnedReservation.Guest.JMBG, Is.EqualTo("1234512345123"));
    }

    [Test]
    public async Task GetReservationById_InvalidId_ReturnsNotFound()
    {
        Reservation reservation = new Reservation
        {
            ReservationID = 1,
            RoomNumber = 699,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3)
        };
        _context.Reservations.Add(reservation);
        _context.SaveChanges();

        _context.Reservations.Add(new Reservation
        {
            ReservationID = 2,
            RoomNumber = 500,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3),
            TotalPrice = 320.00m,
            RoomServices = { _roomService },
            ExtraServices = { _extraService }
        });

        _context.SaveChanges();

        var result = await _controllerReservation.GetReservationById(999);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFound = result as NotFoundObjectResult;
        Assert.That(notFound.Value, Is.EqualTo("Reservation with ID 999 not found."));
    }

    [Test]
    public async Task GetReservationById_ValidId_ReturnsEmptyServiceLists()
    {
        Reservation reservation = new Reservation
        {
            ReservationID = 1,
            RoomNumber = 699,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3)
        };
        _context.Reservations.Add(reservation);
        _context.SaveChanges();

        var result = await _controllerReservation.GetReservationById(1);
        var okResult = result as OkObjectResult;

        var returnedReservation = okResult.Value as Reservation;
        Assert.That(returnedReservation.RoomServices, Is.Empty);
        Assert.That(returnedReservation.ExtraServices, Is.Empty);
    }

    [Test]
    public async Task GetReservationById_WithServices_ReturnsReservationWithAllIncluded()
    {
        _context.Reservations.Add(new Reservation
        {
            ReservationID = 2,
            RoomNumber = 500,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3),
            TotalPrice = 320.00m,
            RoomServices = { _roomService },
            ExtraServices = { _extraService }
        });

        _context.SaveChanges();
        var result = await _controllerReservation.GetReservationById(2);
        var okResult = result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);
        var reservation = okResult.Value as Reservation;

        Assert.That(reservation, Is.Not.Null);
        
        Assert.That(reservation.Guest, Is.Not.Null);
        Assert.That(reservation.Room, Is.Not.Null);

        Assert.That(reservation.RoomServices, Is.Not.Null.And.Count.EqualTo(1));
        Assert.That(reservation.ExtraServices, Is.Not.Null.And.Count.EqualTo(1));

        Assert.That(reservation.RoomServices[0].ItemName, Is.EqualTo("Breakfast"));
        Assert.That(reservation.ExtraServices[0].ServiceName, Is.EqualTo("Parking Spot"));

        Assert.That(reservation.GuestID, Is.EqualTo("1234512345123"));
        Assert.That(reservation.RoomNumber, Is.EqualTo(500));
    }

    [Test]
    public async Task GetReservationById_ValidatesCheckInAndCheckOutDates()
    {
        
        _context.Reservations.Add(new Reservation
        {
            ReservationID = 2,
            RoomNumber = 500,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3),
            TotalPrice = 320.00m,
            RoomServices = { _roomService },
            ExtraServices = { _extraService }
        });

        _context.SaveChanges();
        var result = await _controllerReservation.GetReservationById(2);
        var okResult = result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);
        var reservation = okResult.Value as Reservation;

        Assert.That(reservation, Is.Not.Null);

        Assert.That(reservation.CheckInDate, Is.EqualTo(new DateTime(2025, 9, 1)));

        Assert.That(reservation.CheckOutDate, Is.EqualTo(new DateTime(2025, 9, 3)));
    }

    [Test]
    public async Task GetAllReservations_ReturnsTwoReservations()
    {
        Reservation reservation = new Reservation
        {
            ReservationID = 1,
            RoomNumber = 699,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3)
        };
        _context.Reservations.Add(reservation);
        _context.SaveChanges();

        _context.Reservations.Add(new Reservation
        {
            ReservationID = 2,
            RoomNumber = 500,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3),
            TotalPrice = 320.00m,
            RoomServices = { _roomService },
            ExtraServices = { _extraService }
        });

        _context.SaveChanges();

        var result = await _controllerReservation.GetAllReservations();
        var okResult = result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);
        
        var reservations = okResult.Value as List<Reservation>;

        Assert.That(reservations, Is.Not.Null);
        Assert.That(reservations.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetAllReservations_NoReservationsExist_ReturnsNotFound()
    {
        var result = await _controllerReservation.GetAllReservations();
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Has.Property("Value").EqualTo($"No reservations found!"));
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

