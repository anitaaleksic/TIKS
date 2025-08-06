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

public class RoomServiceController_PutRoomService_Tests
{
    private static HotelContext _context;
    private static RoomServiceController _controllerRoomService;

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

        _context.RoomServices.Add(new RoomService
        {
            RoomServiceID = 1,
            ItemName = "Breakfast",
            ItemPrice = 10m,
            Description = "Continental breakfast"
        });

        _context.SaveChanges();

        _context.RoomServices.Add(new RoomService
        {
            RoomServiceID = 2,
            ItemName = "Laundry",
            ItemPrice = 15m,
            Description = "Laundry service"
        });

        _context.SaveChanges();

    }
    [Test]
    public async Task UpdateRoomServiceByName_ValidUpdate_ReturnsOk()
    {
        var dto = new RoomServiceDTO
        {
            ItemName = "Laundry Updated",
            ItemPrice = 18,
            Description = "Updated description"
        };

        var result = await _controllerRoomService.UpdateRoomServiceByName("Laundry", dto);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var ok = result as OkObjectResult;
        Assert.That(ok?.Value, Is.EqualTo("Room service with name Laundry updated successfully."));

        var updated = await _context.RoomServices.FirstOrDefaultAsync(rs => rs.RoomServiceID == 2);
        Assert.That(updated.ItemName, Is.EqualTo(dto.ItemName));
        Assert.That(updated.ItemPrice, Is.EqualTo(dto.ItemPrice));
        Assert.That(updated.Description, Is.EqualTo(dto.Description));
    }

    [Test]
    public async Task UpdateRoomServiceByName_ServiceNotFound_ReturnsNotFound()
    {
        var dto = new RoomServiceDTO
        {
            ItemName = "New Service",
            ItemPrice = 10,
            Description = "blabla"
        };

        var result = await _controllerRoomService.UpdateRoomServiceByName("New Service", dto);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFound = result as NotFoundObjectResult;
        Assert.That(notFound?.Value, Is.EqualTo("Room service with name New Service not found."));
    }

    [Test] 
    public async Task UpdateRoomService_WithModelStateInvalid_ReturnsBadRequest()
    {
        // Arrange
        _controllerRoomService.ModelState.AddModelError("error", "some model state error");
        var roomServiceDTO = new RoomServiceDTO();
        var someValidID = 1;

        var result = await _controllerRoomService.UpdateRoomService(someValidID, roomServiceDTO);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateRoomServiceByName_ItemPriceLessThanOrEqualZero_ReturnsBadRequest()
    {
        var dto = new RoomServiceDTO
        {
            ItemName = "Laundry Updated",
            ItemPrice = 0, // Nije dozvoljeno
            Description = "Description"
        };

        var result = await _controllerRoomService.UpdateRoomServiceByName("Laundry", dto);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badReq = result as BadRequestObjectResult;
        Assert.That(badReq?.Value, Is.EqualTo("Price must be a positive value."));
    }

    [Test]
    public async Task UpdateRoomServiceByName_ItemNameAlreadyExists_ReturnsBadRequest()
    {
        var dto = new RoomServiceDTO
        {
            ItemName = "Breakfast", // Ovo ime već postoji u bazi na drugom ID-u
            ItemPrice = 25,
            Description = "New desc"
        };

        var result = await _controllerRoomService.UpdateRoomServiceByName("Laundry", dto);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badReq = result as BadRequestObjectResult;
        Assert.That(badReq?.Value, Is.EqualTo("Room service with the name Breakfast already exists."));
    }

    [Test]
    public async Task UpdateRoomServiceByName_EmptyItemName_ReturnsBadRequest()
    {
        var dtoEmpty = new RoomServiceDTO
        {
            ItemName = "",
            ItemPrice = 10,
            Description = null
        };

        var result = await _controllerRoomService.UpdateRoomServiceByName("Laundry", dtoEmpty);
        var badRequestResult = result as BadRequestObjectResult;

        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.Value, Is.EqualTo("Service name is required and cannot exceed 50 characters."));
    }

    [Test]
    public async Task UpdateRoomServiceByName_TooLongItemName_ReturnsBadRequestWithCorrectMessage()
    {
        var dtoTooLong = new RoomServiceDTO
        {
            ItemName = new string('a', 101), // 101 karakter
            ItemPrice = 10,
            Description = null
        };

        var result = await _controllerRoomService.UpdateRoomServiceByName("Laundry", dtoTooLong);
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