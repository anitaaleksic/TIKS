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
public class GuestController_UpdateGuest_Tests
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

        //seedDb
        _context.Guests.Add(new Guest { JMBG = "1234512345123", FullName = "Anita Aleksic", PhoneNumber = "+381651234567" });
        _context.SaveChanges();
        
    }

    [Test] //add to all Create and Update tests
    public async Task UpdateGuest_WithModelStateInvalid_ReturnsBadRequest()
    {
        // Arrange
        _controllerGuest.ModelState.AddModelError("error", "some model state error");
        var guestDTO = new GuestDTO();
        var someValidJmbg = "1234512345123";

        var result = await _controllerGuest.UpdateGuest(someValidJmbg, guestDTO);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    //oba ne mogu jer se ne menja jmbg pri updateu
    // [Test]
    // public async Task UpdateGuest_WithJMBGTooLong_ReturnBadRequest()
    // {       
    //     var guestDTO = new GuestDTO { FullName = "test", JMBG = "12345678912345", PhoneNumber = "+381655455454" };
    //     var result = await _controllerGuest.UpdateGuest("1234512345123", guestDTO);

    //     Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    //     var badReqRes = result as BadRequestObjectResult;
    //     Assert.That(badReqRes, Has.Property("Value").EqualTo("JMBG is required and must have exactly 13 characters."));
    // }

    // [Test]
    // public async Task UpdateGuest_WithJMBGTooShort_ReturnBadRequest()
    // {
    //     var guestDTO = new GuestDTO { FullName = "test", JMBG = "123456789123", PhoneNumber = "+381655455454" };
    //     var result = await _controllerGuest.UpdateGuest("1234512345123", guestDTO);

    //     Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    //     var badReqRes = result as BadRequestObjectResult;
    //     Assert.That(badReqRes, Has.Property("Value").EqualTo("JMBG is required and must have exactly 13 characters."));
    // }



    [Test]
    public async Task UpdateGuest_WithFullNameTooLong_ReturnBadRequest()
    {
        string name = new string('a', 101);
        var guestDTO = new GuestDTO { FullName = name, JMBG = "1234567891234", PhoneNumber = "+381655455454" };
        var result = await _controllerGuest.UpdateGuest("1234512345123", guestDTO);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badReqRes = result as BadRequestObjectResult;
        Assert.That(badReqRes, Has.Property("Value").EqualTo("Full name is required and cannot exceed 100 characters."));
    }

    [Test]
    public async Task UpdateGuest_WithFullNameEmpty_ReturnBadRequest()
    {
        var guestDTO = new GuestDTO { FullName = "", JMBG = "1234567891234", PhoneNumber = "+381655455454" };
        var result = await _controllerGuest.UpdateGuest("1234512345123", guestDTO);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badReqRes = result as BadRequestObjectResult;
        Assert.That(badReqRes, Has.Property("Value").EqualTo("Full name is required and cannot exceed 100 characters."));
    }

    [Test]
    public async Task UpdateGuest_WithPhoneNumberTooLong_ReturnBadRequest()
    {
        var guestDTO = new GuestDTO { FullName = "test", JMBG = "1234567891234", PhoneNumber = "+3816444444444" };
        var result = await _controllerGuest.UpdateGuest("1234512345123", guestDTO);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badReqRes = result as BadRequestObjectResult;
        Assert.That(badReqRes, Has.Property("Value").EqualTo("Phone number is required and must be between 12 and 13 characters long."));
    }

    [Test]
    public async Task UpdateGuest_WithPhoneNumberTooShort_ReturnBadRequest()
    {
        var guestDTO = new GuestDTO { FullName = "test", JMBG = "1234567891234", PhoneNumber = "+3816444444" };
        var result = await _controllerGuest.UpdateGuest("1234512345123", guestDTO);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badReqRes = result as BadRequestObjectResult;
        Assert.That(badReqRes, Has.Property("Value").EqualTo("Phone number is required and must be between 12 and 13 characters long."));
    }

    [Test]
    public async Task UpdateGuest_WithPhoneNumberEmpty_ReturnBadRequest()
    {
        var guestDTO = new GuestDTO { FullName = "test", JMBG = "1234567891234", PhoneNumber = "" };
        var result = await _controllerGuest.UpdateGuest("1234512345123", guestDTO);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badReqRes = result as BadRequestObjectResult;
        Assert.That(badReqRes, Has.Property("Value").EqualTo("Phone number is required and must be between 12 and 13 characters long."));
    }

    [Test]
    public async Task UpdateGuest_WithPhoneNumberInvalid_ReturnBadRequest()
    {
        var guestDTO = new GuestDTO { FullName = "test", JMBG = "1234567891234", PhoneNumber = "+381644$44444" };
        var result = await _controllerGuest.UpdateGuest("1234512345123", guestDTO);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badReqRes = result as BadRequestObjectResult;
        Assert.That(badReqRes, Has.Property("Value").EqualTo("Invalid phone number."));
    }

    [Test]
    public async Task UpdateGuest_WithNonExistingId_ReturnsNotFound()
    {
        string nonExistingId = "9999999999999";
        var guestDTO = new GuestDTO { FullName = "test", JMBG = "1234567891234", PhoneNumber = "+381644$44444" };
        var result = await _controllerGuest.UpdateGuest(nonExistingId, guestDTO);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Has.Property("Value").EqualTo($"Guest with JMBG {nonExistingId} not found."));
    }

    [Test]
    public async Task UpdateGuest_WithEmptyInput_ReturnBadRequest()
    {
        var guestDTO = new GuestDTO { FullName = "test", JMBG = "1234567891234", PhoneNumber = "+381644$44444" };
        var result = await _controllerGuest.UpdateGuest("", guestDTO);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badReqRes = result as BadRequestObjectResult;
        Assert.That(badReqRes, Has.Property("Value").EqualTo("JMBG must be exactly 13 characters long."));
    }

    [Test]//koristi i get vrv ne bi trebalo 
    public async Task UpdateGuest_AllValid_UpdatesGuest()
    {
        string validJmbg = "1234512345123";
        var guestDTO = new GuestDTO { FullName = "test", JMBG = "9999999999999", PhoneNumber = "+381644544444" };
        var result = await _controllerGuest.UpdateGuest(validJmbg, guestDTO);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Has.Property("Value").EqualTo($"Guest with JMBG {validJmbg} updated successfully."));

        var res = await _controllerGuest.GetGuestByJMBG(validJmbg);
        var updatedRes = res as OkObjectResult;
        var updatedGuest = updatedRes.Value as GuestDTO;
        Assert.That(updatedGuest.FullName, Is.EqualTo(guestDTO.FullName));
        Assert.That(updatedGuest.PhoneNumber, Is.EqualTo(guestDTO.PhoneNumber));
        Assert.That(updatedGuest.JMBG, Is.EqualTo(validJmbg));//jmbg not changed
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}