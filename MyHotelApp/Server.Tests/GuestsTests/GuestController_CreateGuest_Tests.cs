using Microsoft.EntityFrameworkCore;
using MyHotelApp.server.Models;
using MyHotelApp.Controllers;
using Microsoft.AspNetCore.Mvc;

using NUnit.Framework;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Collections.Generic;
using Microsoft.Identity.Client;
using System.Net.NetworkInformation;

namespace GuestTests;

[TestFixture]
public class GuestController_CreateGuest_Tests
{
    private static HotelContext _context;
    private static GuestController _controllerGuest;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<HotelContext>()
                    .UseInMemoryDatabase(databaseName: "HotelTestDb")
                    .Options;
        _context = new HotelContext(options);

        _controllerGuest = new GuestController(_context);
    }

    [Test] 
    public async Task CreateGuest_WithModelStateInvalid_ReturnsBadRequest()
    {
        _controllerGuest.ModelState.AddModelError("error", "some model state error");
        var guestDTO = new GuestDTO();

        var result = await _controllerGuest.CreateGuest(guestDTO);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task CreateGuest_WithJMBGTooLong_ReturnBadRequest()
    {
        var guestDTO = new GuestDTO { FullName = "test", JMBG = "12345678912345", PhoneNumber = "+381655455454" };
        var result = await _controllerGuest.CreateGuest(guestDTO);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badReqRes = result as BadRequestObjectResult;
        Assert.That(badReqRes, Has.Property("Value").EqualTo("JMBG must be exactly 13 characters long."));
    }

    [Test]
    public async Task CreateGuest_WithJMBGTooShort_ReturnBadRequest()
    {
        var guestDTO = new GuestDTO { FullName = "test", JMBG = "123456789123", PhoneNumber = "+381655455454" };
        var result = await _controllerGuest.CreateGuest(guestDTO);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badReqRes = result as BadRequestObjectResult;
        Assert.That(badReqRes, Has.Property("Value").EqualTo("JMBG must be exactly 13 characters long."));
    }

    [Test]
    public async Task CreateGuest_WithFullNameTooLong_ReturnBadRequest()
    {
        string name = new string('a', 101);
        var guestDTO = new GuestDTO { FullName = name, JMBG = "1234567891234", PhoneNumber = "+381655455454" };
        var result = await _controllerGuest.CreateGuest(guestDTO);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badReqRes = result as BadRequestObjectResult;
        Assert.That(badReqRes, Has.Property("Value").EqualTo("Full name is required and cannot exceed 100 characters."));
    }

    [Test]
    public async Task CreateGuest_WithFullNameEmpty_ReturnBadRequest()
    {
        var guestDTO = new GuestDTO { FullName = "", JMBG = "1234567891234", PhoneNumber = "+381655455454" };
        var result = await _controllerGuest.CreateGuest(guestDTO);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badReqRes = result as BadRequestObjectResult;
        Assert.That(badReqRes, Has.Property("Value").EqualTo("Full name is required and cannot exceed 100 characters."));
    }

    [Test]
    public async Task CreateGuest_WithPhoneNumberTooLong_ReturnBadRequest()
    {
        var guestDTO = new GuestDTO { FullName = "test", JMBG = "1234567891234", PhoneNumber = "+3816444444444" };
        var result = await _controllerGuest.CreateGuest(guestDTO);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badReqRes = result as BadRequestObjectResult;
        Assert.That(badReqRes, Has.Property("Value").EqualTo("Phone number is required and must be between 12 and 13 characters long."));
    }

    [Test]
    public async Task CreateGuest_WithPhoneNumberTooShort_ReturnBadRequest()
    {
        var guestDTO = new GuestDTO { FullName = "test", JMBG = "1234567891234", PhoneNumber = "+3816444444" };
        var result = await _controllerGuest.CreateGuest(guestDTO);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badReqRes = result as BadRequestObjectResult;
        Assert.That(badReqRes, Has.Property("Value").EqualTo("Phone number is required and must be between 12 and 13 characters long."));
    }

    [Test]
    public async Task CreateGuest_WithPhoneNumberEmpty_ReturnBadRequest()
    {
        var guestDTO = new GuestDTO { FullName = "test", JMBG = "1234567891234", PhoneNumber = "" };
        var result = await _controllerGuest.CreateGuest(guestDTO);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badReqRes = result as BadRequestObjectResult;
        Assert.That(badReqRes, Has.Property("Value").EqualTo("Phone number is required and must be between 12 and 13 characters long."));
    }

    [Test]
    public async Task CreateGuest_WithPhoneNumberInvalid_ReturnBadRequest()
    {
        var guestDTO = new GuestDTO { FullName = "test", JMBG = "1234567891234", PhoneNumber = "+381644$4444444" };
        var result = await _controllerGuest.CreateGuest(guestDTO);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badReqRes = result as BadRequestObjectResult;
        Assert.That(badReqRes, Has.Property("Value").EqualTo("Invalid phone number."));
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}