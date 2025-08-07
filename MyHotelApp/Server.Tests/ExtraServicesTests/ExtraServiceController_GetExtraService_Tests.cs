using Microsoft.EntityFrameworkCore;
using MyHotelApp.server.Models;
using MyHotelApp.Controllers;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ExtraServiceTests;

[TestFixture]

public class ExtraServiceController_GetExtraService_Tests
{
    private static HotelContext _context;
    private static ExtraServiceController _controllerExtraService;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<HotelContext>()
                    .UseInMemoryDatabase(databaseName: "HotelTestDb")
                    //.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionWarning))
                    .Options;
        _context = new HotelContext(options);

        _controllerExtraService = new ExtraServiceController(_context);
        // _controllerReservation = new ReservationController(_context);


    }
    [Test]
    public async Task GetExtraServiceByName_ExistingName_ReturnsOkWithCorrectData()
    {
        var service = new ExtraService
        {
            ServiceName = "Wellness Access",
            Price = 30m,
            Description = "Access to wellness center"
        };
        await _context.ExtraServices.AddAsync(service);
        await _context.SaveChangesAsync();

        var result = await _controllerExtraService.GetExtraServiceByName("Wellness Access");

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var ok = result as OkObjectResult;
        var returned = ok?.Value as ExtraService;
        Assert.That(returned, Is.Not.Null);
        Assert.That(returned?.ServiceName, Is.EqualTo("Wellness Access"));
        Assert.That(returned?.Price, Is.EqualTo(30m));
        Assert.That(returned?.Description, Is.EqualTo("Access to wellness center"));
    }

    [Test]
    public async Task GetExtraServiceByName_NonExistentName_ReturnsNotFound()
    {
        var result = await _controllerExtraService.GetExtraServiceByName("NonExistent");

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFound = result as NotFoundObjectResult;
        Assert.That(notFound?.Value, Is.EqualTo("Extra service with name NonExistent not found."));
    }

    [Test]
    public async Task GetExtraServiceByName_EmptyString_ReturnsNotFound()
    {
        var result = await _controllerExtraService.GetExtraServiceByName("");

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFound = result as NotFoundObjectResult;
        Assert.That(notFound?.Value, Is.EqualTo("Extra service with name  not found."));
    }

    [Test]
    public async Task GetExtraServiceByName_WithExistingName_ReturnsCorrectServiceName()
    {
        var expectedName = "WiFi Access";
        await _context.ExtraServices.AddAsync(new ExtraService
        {
            ServiceName = expectedName,
            Price = 10m,
            Description = "Free WiFi"
        });
        await _context.SaveChangesAsync();

        var result = await _controllerExtraService.GetExtraServiceByName(expectedName);
        var okResult = result as OkObjectResult;
        var service = okResult?.Value as ExtraService;

        Assert.That(service, Has.Property("ServiceName").EqualTo(expectedName));
    }
    [Test]
    public async Task GetExtraServiceByName_WithExistingName_ReturnsCorrectPrice()
    {
        var name = "Car Rental";
        var expectedPrice = 100m;

        await _context.ExtraServices.AddAsync(new ExtraService
        {
            ServiceName = name,
            Price = expectedPrice,
            Description = "Car rental service"
        });
        await _context.SaveChangesAsync();

        var result = await _controllerExtraService.GetExtraServiceByName(name);
        var okResult = result as OkObjectResult;
        var service = okResult?.Value as ExtraService;

        Assert.That(service, Has.Property("Price").EqualTo(expectedPrice));
    }

    [Test]
    public async Task GetExtraServiceByName_WithExistingName_ReturnsCorrectDescription()
    {
        var name = "Laundry Service";
        var expectedDescription = "Laundry for clothes";

        await _context.ExtraServices.AddAsync(new ExtraService
        {
            ServiceName = name,
            Price = 10.00m,
            Description = expectedDescription
        });
        await _context.SaveChangesAsync();

        var result = await _controllerExtraService.GetExtraServiceByName(name);
        var okResult = result as OkObjectResult;
        var service = okResult?.Value as ExtraService;

        Assert.That(service, Has.Property("Description").EqualTo(expectedDescription));
    }

    [Test]
    public async Task GetExtraService_HasReservations_ReturnsExtraServiceWithReservations()
    {
        
        var extraService = new ExtraService
        {
            ExtraServiceID = 1,
            ServiceName = "Spa Access",
            Price = 30.0m,
            Description = "Daily spa usage"
        };

        var guest = new Guest
        {
            JMBG = "9998887776665",
            FullName = "Spa Guest",
            PhoneNumber = "+381601112233"
        };

        var room = new Room
        {
            RoomNumber = 303,
            RoomTypeID = 2,
            Floor = 3
        };

        var reservation = new Reservation
        {
            ReservationID = 1,
            RoomNumber = 303,
            GuestID = guest.JMBG,
            CheckInDate = new DateTime(2025, 11, 1),
            CheckOutDate = new DateTime(2025, 11, 5),
            TotalPrice = 500m,
            ExtraServices = new List<ExtraService> { extraService } // povezivanje m:n
        };

        _context.Guests.Add(guest);
        _context.Rooms.Add(room);
        _context.ExtraServices.Add(extraService);
        _context.Reservations.Add(reservation);
        _context.SaveChanges();

        
        var result = await _controllerExtraService.GetExtraServiceById(1);
        var okResult = result as OkObjectResult;
        var es = okResult?.Value as ExtraService;

        Assert.That(okResult, Is.Not.Null);
        Assert.That(es, Is.Not.Null);
        Assert.That(es!.AddedToReservations, Is.Not.Null);
        Assert.That(es.AddedToReservations.Count, Is.EqualTo(1));
    }
    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}