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

public class ExtraServiceController_DeleteExtraService_Tests
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

        _context.ExtraServices.Add(new ExtraService
        {
            ExtraServiceID = 1,
            ServiceName = "Parking Spot",
            Price = 10m,
            Description = "Reserved parking space"
        });

        _context.SaveChanges();

        _context.ExtraServices.Add(new ExtraService
        {
            ExtraServiceID = 2,
            ServiceName = "Restaurant Access",
            Price = 25m,
            Description = "Access to hotel restaurant"
        });

        _context.SaveChanges();

    }

    [Test]
    public async Task DeleteExtraService_WithValidId_ReturnsOkAndDeletesService()
    {
        var service = new ExtraService
        {
            ServiceName = "Wellness Access",
            Price = 30m,
            Description = "Access to wellness center"
        };
        await _context.ExtraServices.AddAsync(service);
        await _context.SaveChangesAsync();

        var result = await _controllerExtraService.DeleteExtraService(service.ExtraServiceID);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var ok = result as OkObjectResult;
        Assert.That(ok?.Value, Is.EqualTo($"Extra service with ID {service.ExtraServiceID} deleted successfully."));

        var deleted = await _context.ExtraServices.FindAsync(service.ExtraServiceID);
        Assert.That(deleted, Is.Null);
    }

    [Test]
    public async Task DeleteExtraService_WithNonExistingId_ReturnsNotFound()
    {
        var nonExistingId = 999;

        var result = await _controllerExtraService.DeleteExtraService(nonExistingId);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFound = result as NotFoundObjectResult;
        Assert.That(notFound?.Value, Is.EqualTo($"Extra service with ID {nonExistingId} not found."));
    }

    [Test]
    public async Task DeleteExtraService_Twice_SecondTimeReturnsNotFound()
    {
        var service = new ExtraService
        {
            ServiceName = "Wellness Access",
            Price = 30m,
            Description = "Access to wellness center"
        };
        await _context.ExtraServices.AddAsync(service);
        await _context.SaveChangesAsync();

        var firstDelete = await _controllerExtraService.DeleteExtraService(service.ExtraServiceID);
        Assert.That(firstDelete, Is.InstanceOf<OkObjectResult>());

        var secondDelete = await _controllerExtraService.DeleteExtraService(service.ExtraServiceID);
        Assert.That(secondDelete, Is.InstanceOf<NotFoundObjectResult>());
        var notFound = secondDelete as NotFoundObjectResult;
        Assert.That(notFound?.Value, Is.EqualTo($"Extra service with ID {service.ExtraServiceID} not found."));
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}