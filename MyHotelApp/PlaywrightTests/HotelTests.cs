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

namespace PlaywrightTests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]

public class HotelTests : PageTest
{
    private static HotelContext _context;

    [SetUp]
    public async Task SetUp()
    {
        var optionsBuilder = new DbContextOptionsBuilder<HotelContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\HotelDatabase;Database=Hotel");
        var options = optionsBuilder.Options;
        _context = new HotelContext(options);

        //refreshing db
        //await DataBaseRefresher.AddDataAsync(_context);
    }

    [Test]
    public async Task AddGuest()
    {
        await Page.GotoAsync("http://localhost:5173/guest");//5173
        await Page.Locator("input[name=\"fullName\"]").ClickAsync();
        await Page.Locator("input[name=\"fullName\"]").FillAsync("Anita Aleksic");
        await Page.Locator("input[name=\"jmbg\"]").ClickAsync();
        await Page.Locator("input[name=\"jmbg\"]").FillAsync("1234512345123");
        await Page.Locator("input[name=\"phoneNumber\"]").ClickAsync();
        await Page.Locator("input[name=\"phoneNumber\"]").FillAsync("+381644444444");
        await Page.GetByRole(AriaRole.Button, new() { NameString = "Add guest" }).ClickAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
