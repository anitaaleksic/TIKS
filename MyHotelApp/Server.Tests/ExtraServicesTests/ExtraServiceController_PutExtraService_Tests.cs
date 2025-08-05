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

public class ExtraServiceController_PutExtraService_Tests
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
    public async Task UpdateExtraServiceByName_ValidUpdate_ReturnsOk()
    {
        var dto = new ExtraServiceDTO
        {
            ServiceName = "Wellness Access",
            Price = 30m,
            Description = "Access to wellness center"
        };

        var result = await _controllerExtraService.UpdateExtraServiceByName("Restaurant Access", dto);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var ok = result as OkObjectResult;
        Assert.That(ok?.Value, Is.EqualTo("Extra service with name Restaurant Access updated successfully."));

        var updated = await _context.ExtraServices.FirstOrDefaultAsync(rs => rs.ExtraServiceID == 2);
        Assert.That(updated.ServiceName, Is.EqualTo(dto.ServiceName));
        Assert.That(updated.Price, Is.EqualTo(dto.Price));
        Assert.That(updated.Description, Is.EqualTo(dto.Description));
    }

    [Test]
    public async Task UpdateExtraServiceByName_ServiceNotFound_ReturnsNotFound()
    {
        var dto = new ExtraServiceDTO
        {
            ServiceName = "New Service",
            Price = 10,
            Description = "blabla"
        };

        var result = await _controllerExtraService.UpdateExtraServiceByName("New Service", dto);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFound = result as NotFoundObjectResult;
        Assert.That(notFound?.Value, Is.EqualTo("Extra service with name New Service not found."));
    }

    [Test]
    public async Task UpdateExtraServiceByName_InvalidModelState_ReturnsBadRequest()
    {
        _controllerExtraService.ModelState.AddModelError("Price", "Required");

        var dto = new ExtraServiceDTO
        {
            ServiceName = "Restaurant Access Updated",
            Price = 0,
            Description = "Description"
        };

        var result = await _controllerExtraService.UpdateExtraServiceByName("Restaurant Access", dto);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateExtraServiceByName_PriceLessThanOrEqualZero_ReturnsBadRequest()
    {
        var dto = new ExtraServiceDTO
        {
            ServiceName = "Restaurant Access Updated",
            Price = 0, // Nije dozvoljeno
            Description = "Description"
        };

        var result = await _controllerExtraService.UpdateExtraServiceByName("Restaurant Access", dto);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badReq = result as BadRequestObjectResult;
        Assert.That(badReq?.Value, Is.EqualTo("Price must be a positive value."));
    }

    [Test]
    public async Task UpdateExtraServiceByName_ServiceNameAlreadyExists_ReturnsBadRequest()
    {
        var dto = new ExtraServiceDTO
        {
            ServiceName = "Restaurant Access", // Ovo ime već postoji u bazi na drugom ID-u
            Price = 25,
            Description = "New desc"
        };

        var result = await _controllerExtraService.UpdateExtraServiceByName("Parking Spot", dto);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badReq = result as BadRequestObjectResult;
        Assert.That(badReq?.Value, Is.EqualTo("Extra service with the name Restaurant Access already exists."));
    }

    [Test]
    public async Task UpdateExtraServiceByName_EmptyServiceName_ReturnsBadRequest()
    {
        var dtoEmpty = new ExtraServiceDTO
        {
            ServiceName = "",
            Price = 10,
            Description = null
        };

        var result = await _controllerExtraService.UpdateExtraServiceByName("Restaurant Access", dtoEmpty);
        var badRequestResult = result as BadRequestObjectResult;

        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.Value, Is.EqualTo("Service name is required and cannot exceed 50 characters."));
    }

    [Test]
    public async Task UpdateExtraServiceByName_TooLongServiceName_ReturnsBadRequestWithCorrectMessage()
    {
        var dtoTooLong = new ExtraServiceDTO
        {
            ServiceName = new string('a', 101), // 101 karakter
            Price = 10,
            Description = null
        };

        var result = await _controllerExtraService.UpdateExtraServiceByName("Restaurant Access", dtoTooLong);
        var badRequestResult = result as BadRequestObjectResult;

        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.Value, Is.EqualTo("Service name is required and cannot exceed 50 characters."));
    }
    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}