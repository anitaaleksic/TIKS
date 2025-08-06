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

public class ExtraServiceController_GetAllExtraServices_Tests
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
    public async Task GetAllExtraServices_ExtraServicesExist_ReturnsList()
    {
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
        

        var result = await _controllerExtraService.GetAllExtraServices();
        var okResult = result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);
        var extraServices = okResult.Value as List<ExtraService>;
        Assert.That(extraServices, Is.Not.Null);
        Assert.That(extraServices.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetAllExtraServices_NoExtraServicesExist_ReturnsNotFound()
    {
        var result = await _controllerExtraService.GetAllExtraServices();
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Has.Property("Value").EqualTo($"No extra services found."));
    }

    [Test]
    public async Task GetAllExtraServices_ReturnsOnlyExtraServiceObjects()
    {
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

        var result = await _controllerExtraService.GetAllExtraServices();
        var okResult = result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);
        var extraServices = okResult?.Value as List<ExtraService>;
        Assert.That(extraServices, Is.Not.Null);

        foreach (var extraService in extraServices!)
        {
            Assert.That(extraService, Is.TypeOf<ExtraService>());
        }
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}