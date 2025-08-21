using Microsoft.Playwright.NUnit;
using Microsoft.Playwright;

using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PlaywrightTests;
using NUnit.Framework;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyHotelApp.server.Models;
using MyHotelApp.Controllers;

using System.Threading.Tasks;

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text;


namespace PlaywrightTests.APITests;



[TestFixture]
public class GuestControllerTests : PlaywrightTest
{
    private IAPIRequestContext Request;
    private HotelContext _context;
    //var page = await context.NewPageAsync();

    // public override BrowserNewContextOptions ContextOptions()
    // {
    //     return new BrowserNewContextOptions
    //     {
    //         IgnoreHTTPSErrors = true,
    //         ViewportSize = new ViewportSize
    //         {
    //             Height = 720,
    //             Width = 1280
    //         },
    //         RecordVideoSize = new()
    //         {
    //             Width = 1280,
    //             Height = 720
    //         },
    //         RecordVideoDir = "../../../Videos"
    //     };
    // }

    [SetUp]
    public async Task SetUpAPITesting()
    {
        var headers = new Dictionary<string, string>
        {
            {"Accept", "application/json"},
        };

        Request = await Playwright.APIRequest.NewContextAsync(new()
        {
            BaseURL = "http://localhost:5023",
            ExtraHTTPHeaders = headers,
            IgnoreHTTPSErrors = true
        });

        var optionsBuilder = new DbContextOptionsBuilder<HotelContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\HotelDatabase;Database=Hotel");

        var options = optionsBuilder.Options;
        _context = new HotelContext(options);

        // Refresh the database
        await DatabaseRefresher.AddDataAsync(_context);
    }

    [Test]
    public async Task GetGuests_NonEmptyGuests_ReturnsOk()
    {
        await using var response = await Request.GetAsync("/api/Guest/GetAllGuests");

        if (response.Status != 200)
        {
            Assert.Fail($"Code: {response.Status} - {response.StatusText}");
        }

        var jsonResult = await response.JsonAsync();

        if (!jsonResult.HasValue || jsonResult.Value.ValueKind != System.Text.Json.JsonValueKind.Array)
        {
            Assert.Fail("JSON response is not a valid array.");
        }

        var guests = jsonResult.Value.EnumerateArray().ToList();

        Assert.That(guests.Count, Is.GreaterThan(0), "Expected at least one guest in the response.");

        var firstGuest = guests.First();
        Assert.Multiple(() =>
        {
            Assert.That(firstGuest.TryGetProperty("fullName", out var name) && !string.IsNullOrEmpty(name.GetString()));
            Console.WriteLine($"Full Name: {name.GetString()}");
            Assert.That(firstGuest.TryGetProperty("jmbg", out var jmbg) && !string.IsNullOrEmpty(jmbg.GetString()));
            Assert.That(firstGuest.TryGetProperty("phoneNumber", out var phone) && !string.IsNullOrEmpty(phone.GetString()));
        });

    }

    [TearDown]
    public async Task TearDownAPITesting()
    {
        await Request.DisposeAsync();
        await _context.DisposeAsync();
    }
}
