using Microsoft.EntityFrameworkCore;
using MyHotelApp.server.Models;
using MyHotelApp.Controllers;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace RoomServiceTests;

[TestFixture]

public class RoomServiceController_GetRoomService_Tests
{
    private static HotelContext _context;
    private static RoomServiceController _controllerRoomService;
    private static Reservation _reservation;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<HotelContext>()
                    .UseInMemoryDatabase(databaseName: "HotelTestDb")
                    //.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionWarning))
                    .Options;
        _context = new HotelContext(options);

        _controllerRoomService = new RoomServiceController(_context);
        // _controllerReservation = new ReservationController(_context);


    }
    [Test]
    public async Task GetRoomServiceByName_ExistingName_ReturnsOkWithCorrectData()
    {
        var service = new RoomService
        {
            ItemName = "Spa",
            ItemPrice = 50m,
            Description = "Full body massage"
        };
        await _context.RoomServices.AddAsync(service);
        await _context.SaveChangesAsync();

        var result = await _controllerRoomService.GetRoomServiceByName("Spa");

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var ok = result as OkObjectResult;
        var returned = ok?.Value as RoomService;
        Assert.That(returned, Is.Not.Null);
        Assert.That(returned?.ItemName, Is.EqualTo("Spa"));
        Assert.That(returned?.ItemPrice, Is.EqualTo(50m));
        Assert.That(returned?.Description, Is.EqualTo("Full body massage"));
    }

    [Test]
    public async Task GetRoomServiceByName_NonExistentName_ReturnsNotFound()
    {
        var result = await _controllerRoomService.GetRoomServiceByName("NonExistent");

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFound = result as NotFoundObjectResult;
        Assert.That(notFound?.Value, Is.EqualTo("Room service with name NonExistent not found."));
    }

    [Test]
    public async Task GetRoomServiceByName_EmptyString_ReturnsNotFound()
    {
        var result = await _controllerRoomService.GetRoomServiceByName("");

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFound = result as NotFoundObjectResult;
        Assert.That(notFound?.Value, Is.EqualTo("Room service with name  not found."));
    }

    [Test]
    public async Task GetRoomServiceByName_WithExistingName_ReturnsCorrectItemName()
    {
        var expectedName = "Dinner";
        await _context.RoomServices.AddAsync(new RoomService
        {
            ItemName = expectedName,
            ItemPrice = 25.50m,
            Description = "Evening meal"
        });
        await _context.SaveChangesAsync();

        var result = await _controllerRoomService.GetRoomServiceByName(expectedName);
        var okResult = result as OkObjectResult;
        var service = okResult?.Value as RoomService;

        Assert.That(service, Has.Property("ItemName").EqualTo(expectedName));
    }
    [Test]
    public async Task GetRoomServiceByName_WithExistingName_ReturnsCorrectItemPrice()
    {
        var name = "Lunch";
        var expectedPrice = 19.99m;

        await _context.RoomServices.AddAsync(new RoomService
        {
            ItemName = name,
            ItemPrice = expectedPrice,
            Description = "Afternoon meal"
        });
        await _context.SaveChangesAsync();

        var result = await _controllerRoomService.GetRoomServiceByName(name);
        var okResult = result as OkObjectResult;
        var service = okResult?.Value as RoomService;

        Assert.That(service, Has.Property("ItemPrice").EqualTo(expectedPrice));
    }

    [Test]
    public async Task GetRoomServiceByName_WithExistingName_ReturnsCorrectDescription()
    {
        var name = "Cleaning";
        var expectedDescription = "Daily room cleaning";

        await _context.RoomServices.AddAsync(new RoomService
        {
            ItemName = name,
            ItemPrice = 10.00m,
            Description = expectedDescription
        });
        await _context.SaveChangesAsync();

        var result = await _controllerRoomService.GetRoomServiceByName(name);
        var okResult = result as OkObjectResult;
        var service = okResult?.Value as RoomService;

        Assert.That(service, Has.Property("Description").EqualTo(expectedDescription));
    }


    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}