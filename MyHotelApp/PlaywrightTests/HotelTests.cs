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
        await DatabaseRefresher.AddDataAsync(_context);
    }

    [Test]
    public async Task AddGuest()
    {
        await Page.GotoAsync("http://localhost:5173/guest");//5173
        //await Page.PauseAsync();
        await Page.GetByRole(AriaRole.Button, new() { NameString = "Add Guest" }).ClickAsync();
        //await Page.PauseAsync();
        await Page.Locator("input[name=\"fullName\"]").ClickAsync();
        await Page.Locator("input[name=\"fullName\"]").FillAsync("Anita Aleksic");
        await Page.Locator("input[name=\"jmbg\"]").ClickAsync();
        await Page.Locator("input[name=\"jmbg\"]").FillAsync("1234512345123");
        //await Page.PauseAsync();
        await Page.Locator("input[name=\"phoneNumber\"]").ClickAsync();
        await Page.Locator("input[name=\"phoneNumber\"]").FillAsync("+381644444444");
        //await Page.PauseAsync();
        await Page.GetByRole(AriaRole.Button, new() { NameString = "Add guest" }).ClickAsync();
        //await Page.GotoAsync("http://localhost:5173/guest");//5173
        //await Page.PauseAsync();
        await Page.WaitForURLAsync("**/guest");
        Assert.That(Page.Url, Does.Contain("/guest"));
    }

    [Test]
    public async Task EditGuest()
    {
        await Page.GotoAsync("http://localhost:5173/guest");//5173
        //await Page.PauseAsync();//before
        await Page.Locator("tr[data-jmbg='5678901234567']").ClickAsync();//jelena kovacevic
        //await Page.PauseAsync();
        await Page.Locator("input[name=\"fullName\"]").ClickAsync();
        await Page.Locator("input[name=\"fullName\"]").FillAsync("Anita Aleksic");
        var jmbgField = Page.Locator("input[name='jmbg']");
        await Expect(jmbgField).ToBeDisabledAsync();
        //await Page.PauseAsync();
        await Expect(jmbgField).ToHaveValueAsync("5678901234567");
        //await Page.PauseAsync();
        await Page.Locator("input[name=\"phoneNumber\"]").ClickAsync();
        await Page.Locator("input[name=\"phoneNumber\"]").FillAsync("+381644444444");
        //await Page.PauseAsync();
        await Page.GetByRole(AriaRole.Button, new() { NameString = "Update guest" }).ClickAsync();
        //await Page.GotoAsync("http://localhost:5173/guest");//5173
        //await Page.PauseAsync();//after
        await Page.WaitForURLAsync("**/guest");
        Assert.That(Page.Url, Does.Contain("/guest"));
    }

    [Test]
    public async Task DeleteGuest()
    {
        await Page.GotoAsync("http://localhost:5173/guest");//5173
        await Page.PauseAsync();
        await Page.Locator("tr[data-jmbg='0123456789012']").ClickAsync();//lazar zivkovic
        //await Page.PauseAsync();
        Page.Dialog += async (_, dialog) =>
        {
            await dialog.AcceptAsync(); // Click OK
        };
        await Page.GetByRole(AriaRole.Button, new() { NameString = "Delete Guest" }).ClickAsync();
        //await Page.PauseAsync();
        //await Page.GotoAsync("http://localhost:5173/guest");//5173
        //await Page.PauseAsync();
        await Page.WaitForURLAsync("**/guest");
        Assert.That(Page.Url, Does.Contain("/guest"));
    }

    [Test]
    public async Task ExitEditGuest()
    {
        await Page.GotoAsync("http://localhost:5173/guest");//5173
        await Page.PauseAsync();
        await Page.Locator("tr[data-jmbg='0123456789012']").ClickAsync();//lazar zivkovic

        //await Page.PauseAsync();
        await Page.GetByRole(AriaRole.Button, new() { NameString = "x" }).ClickAsync();
        // Wait for navigation to /guest
        await Page.WaitForURLAsync("**/guest");
        Assert.That(Page.Url, Does.Contain("/guest"));
        //await Page.PauseAsync();
    }

    [Test]
    public async Task ExitAddGuest()
    {
        await Page.GotoAsync("http://localhost:5173/guest");//5173
        await Page.PauseAsync();
        await Page.GetByRole(AriaRole.Button, new() { NameString = "Add Guest" }).ClickAsync();

        //await Page.PauseAsync();
        await Page.GetByRole(AriaRole.Button, new() { NameString = "x" }).ClickAsync();
        // Wait for navigation to /guest
        await Page.WaitForURLAsync("**/guest");

        // Assert we are on the /guest page
        Assert.That(Page.Url, Does.Contain("/guest"));
        await Page.PauseAsync();
    }

    [Test]
    public async Task AddRoomService()
    {
        await Page.GotoAsync("http://localhost:5173/roomservice");//5173
        //await Page.PauseAsync();
        await Page.GetByRole(AriaRole.Button, new() { NameString = "Add Room Service" }).ClickAsync();
        //await Page.PauseAsync();
        await Page.Locator("input[name=\"itemName\"]").ClickAsync();
        await Page.Locator("input[name=\"itemName\"]").FillAsync("Room Service Item");
        await Page.Locator("input[name=\"itemPrice\"]").ClickAsync();
        await Page.Locator("input[name=\"itemPrice\"]").FillAsync("123.45");
        //await Page.PauseAsync();
        await Page.Locator("textarea[name=\"description\"]").ClickAsync();
        await Page.Locator("textarea[name=\"description\"]").FillAsync("Description for room service item");
        //await Page.PauseAsync();
        await Page.GetByRole(AriaRole.Button, new() { NameString = "Add Room Service" }).ClickAsync();
        // await Page.GotoAsync("http://localhost:5173/roomservice");//5173
        // await Page.PauseAsync();
        await Page.WaitForURLAsync("**/roomservice");
        Assert.That(Page.Url, Does.Contain("/roomservice"));
    }

    [Test]
    public async Task EditRoomService()
    {
        await Page.GotoAsync("http://localhost:5173/roomservice");//5173
        //await Page.PauseAsync();//before
        await Page.Locator("tr[data-item-name='Breakfast']").ClickAsync();
        //await Page.PauseAsync();
        await Page.Locator("input[name=\"itemName\"]").ClickAsync();
        await Page.Locator("input[name=\"itemName\"]").FillAsync("Updated Room Service Item");
        await Page.Locator("input[name='itemPrice']").ClickAsync();
        await Page.Locator("input[name='itemPrice']").FillAsync("100.10");

        //await Page.PauseAsync();
        await Page.Locator("textarea[name=\"description\"]").ClickAsync();
        await Page.Locator("textarea[name=\"description\"]").FillAsync("Updated description for room service item");
        //await Page.PauseAsync();
        await Page.GetByRole(AriaRole.Button, new() { NameString = "Update room service" }).ClickAsync();
        // await Page.GotoAsync("http://localhost:5173/roomservice");//5173
        // await Page.PauseAsync();//after
        await Page.WaitForURLAsync("**/roomservice");
        Assert.That(Page.Url, Does.Contain("/roomservice"));
    }

    [Test]
    public async Task DeleteRoomService()
    {
        await Page.GotoAsync("http://localhost:5173/roomservice");//5173
        await Page.PauseAsync();
        await Page.Locator("tr[data-item-name='Breakfast']").ClickAsync();
        //await Page.PauseAsync();
        Page.Dialog += async (_, dialog) =>
        {
            await dialog.AcceptAsync(); // Click OK
        };
        await Page.GetByRole(AriaRole.Button, new() { NameString = "Delete Room Service" }).ClickAsync();
        await Page.PauseAsync();
        // await Page.GotoAsync("http://localhost:5173/roomservice");//5173
        // await Page.PauseAsync();
        await Page.WaitForURLAsync("**/roomservice");
        Assert.That(Page.Url, Does.Contain("/roomservice"));
    }

    [Test]
    public async Task ExitEditRoomService()
    {
        await Page.GotoAsync("http://localhost:5173/roomservice");//5173
        await Page.PauseAsync();
        await Page.Locator("tr[data-item-name='Breakfast']").ClickAsync();

        //await Page.PauseAsync();
        await Page.GetByRole(AriaRole.Button, new() { NameString = "x" }).ClickAsync();
        // Wait for navigation to /roomservice
        await Page.WaitForURLAsync("**/roomservice");
        Assert.That(Page.Url, Does.Contain("/roomservice"));
        //await Page.PauseAsync();
    }

    [Test]
    public async Task ExitAddRoomService()
    {
        await Page.GotoAsync("http://localhost:5173/roomservice");//5173
        await Page.PauseAsync();
        await Page.GetByRole(AriaRole.Button, new() { NameString = "Add Room Service" }).ClickAsync();

        //await Page.PauseAsync();
        await Page.GetByRole(AriaRole.Button, new() { NameString = "x" }).ClickAsync();
        // Wait for navigation to /roomservice
        await Page.WaitForURLAsync("**/roomservice");

        // Assert we are on the /roomservice page
        Assert.That(Page.Url, Does.Contain("/roomservice"));
        await Page.PauseAsync();
    }

    [Test]
    public async Task AddExtraService()
    {
        await Page.GotoAsync("http://localhost:5173/extraservice");//5173
        //await Page.PauseAsync();
        await Page.GetByRole(AriaRole.Button, new() { NameString = "Add Extra Service" }).ClickAsync();
        //await Page.PauseAsync();
        await Page.Locator("input[name=\"serviceName\"]").ClickAsync();
        await Page.Locator("input[name=\"serviceName\"]").FillAsync("Extra Service Item");
        await Page.Locator("input[name=\"price\"]").ClickAsync();
        await Page.Locator("input[name=\"price\"]").FillAsync("123.45");
        //await Page.PauseAsync();
        await Page.Locator("textarea[name=\"description\"]").ClickAsync();
        await Page.Locator("textarea[name=\"description\"]").FillAsync("Description for extra service item");
        //await Page.PauseAsync();
        await Page.GetByRole(AriaRole.Button, new() { NameString = "Add Extra Service" }).ClickAsync();
        // await Page.GotoAsync("http://localhost:5173/extraservice");//5173
        // await Page.PauseAsync();
        await Page.WaitForURLAsync("**/extraservice");
        Assert.That(Page.Url, Does.Contain("/extraservice"));
    }

    [Test]
    public async Task EditExtraService()
    {
        await Page.GotoAsync("http://localhost:5173/extraservice");//5173
        //await Page.PauseAsync();//before
        await Page.Locator("tr[data-item-name='Parking Spot']").ClickAsync();
        //await Page.PauseAsync();
        await Page.Locator("input[name=\"serviceName\"]").ClickAsync();
        await Page.Locator("input[name=\"serviceName\"]").FillAsync("Updated Extra Service Item");
        await Page.Locator("input[name='price']").ClickAsync();
        await Page.Locator("input[name='price']").FillAsync("100.10");

        //await Page.PauseAsync();
        await Page.Locator("textarea[name=\"description\"]").ClickAsync();
        await Page.Locator("textarea[name=\"description\"]").FillAsync("Updated description for extra service item");
        //await Page.PauseAsync();
        await Page.GetByRole(AriaRole.Button, new() { NameString = "Update extra service" }).ClickAsync();
        // await Page.GotoAsync("http://localhost:5173/extraservice");//5173
        // await Page.PauseAsync();//after
        await Page.WaitForURLAsync("**/extraservice");
        Assert.That(Page.Url, Does.Contain("/extraservice"));
    }

    [Test]
    public async Task DeleteExtraService()
    {
        await Page.GotoAsync("http://localhost:5173/extraservice");//5173
        await Page.PauseAsync();
        await Page.Locator("tr[data-item-name='Parking Spot']").ClickAsync();
        //await Page.PauseAsync();
        Page.Dialog += async (_, dialog) =>
        {
            await dialog.AcceptAsync(); // Click OK
        };
        await Page.GetByRole(AriaRole.Button, new() { NameString = "Delete Extra Service" }).ClickAsync();
        await Page.PauseAsync();
        // await Page.GotoAsync("http://localhost:5173/extraservice");//5173
        // await Page.PauseAsync();
        await Page.WaitForURLAsync("**/extraservice");
        Assert.That(Page.Url, Does.Contain("/extraservice"));
    }

    [Test]
    public async Task ExitEditExtraService()
    {
        await Page.GotoAsync("http://localhost:5173/extraservice");//5173
        await Page.PauseAsync();
        await Page.Locator("tr[data-item-name='Parking Spot']").ClickAsync();

        //await Page.PauseAsync();
       await Page.GetByRole(AriaRole.Button, new() { NameString = "x", Exact = true }).ClickAsync();
        // Wait for navigation to /extraservice
        await Page.WaitForURLAsync("**/extraservice");
        Assert.That(Page.Url, Does.Contain("/extraservice"));
        //await Page.PauseAsync();
    }

    [Test]
    public async Task ExitAddExtraService()
    {
        await Page.GotoAsync("http://localhost:5173/extraservice");//5173
        await Page.PauseAsync();
        await Page.GetByRole(AriaRole.Button, new() { NameString = "Add Extra Service" }).ClickAsync();

        //await Page.PauseAsync();
        await Page.GetByRole(AriaRole.Button, new() { NameString = "x", Exact = true }).ClickAsync();
        // Wait for navigation to /extraservice
        await Page.WaitForURLAsync("**/extraservice");

        // Assert we are on the /extraservice page
        Assert.That(Page.Url, Does.Contain("/extraservice"));
        //await Page.PauseAsync();
    }

    [Test]
    public async Task AddRoom()
    {
        await Page.GotoAsync("http://localhost:5173/room");//5173
        //await Page.PauseAsync();
        await Page.GetByRole(AriaRole.Button, new() { NameString = "Add Room" }).ClickAsync();
        //await Page.PauseAsync();
        await Page.Locator("input[name=\"roomNumber\"]").ClickAsync();
        await Page.Locator("input[name=\"roomNumber\"]").FillAsync("111");
        await Page.Locator("select[name=\"roomTypeID\"]").ClickAsync();
        //await Page.PauseAsync();
        await Page.Locator("select[name=\"roomTypeID\"]").SelectOptionAsync(new SelectOptionValue { Label = "Single" });
        var floorField = Page.Locator("input[name=\"floor\"]");
        var readonlyAttr = await floorField.GetAttributeAsync("readonly");
        Assert.That(readonlyAttr, Is.Not.Null);

        await Expect(floorField).ToHaveValueAsync("1");
        //await Page.PauseAsync();
        await Page.GetByRole(AriaRole.Button, new() { NameString = "Add room" }).ClickAsync();
        //await Page.GotoAsync("http://localhost:5173/guest");//5173
        //await Page.PauseAsync();
        await Page.WaitForURLAsync("**/room");
        Assert.That(Page.Url, Does.Contain("/room"));
    }

    [Test]
    public async Task EditRoom()
    {
        await Page.GotoAsync("http://localhost:5173/room");//5173
                                                           //await Page.PauseAsync();
        await Page.Locator("td[data-roomNumber='201']").ClickAsync();
        //await Page.PauseAsync();
        var roomNumberField = Page.Locator("input[name='roomNumber']");
        await Expect(roomNumberField).ToBeDisabledAsync();
        await Expect(roomNumberField).ToHaveValueAsync("201");

        var floorField = Page.Locator("input[name='floor']");
        await Expect(floorField).ToBeDisabledAsync();
        await Expect(floorField).ToHaveValueAsync("2");

        var roomTypeField = Page.Locator("select[name='roomTypeID']");
        await Expect(roomTypeField).ToHaveValueAsync("2");

        roomTypeField.ClickAsync();
        //await Page.Locator(roomTypeField).ClickAsync();

        //await Page.PauseAsync();
        roomTypeField.SelectOptionAsync(new SelectOptionValue { Label = "Single" });
        //await Page.PauseAsync();

        //await Page.PauseAsync();
        await Page.GetByRole(AriaRole.Button, new() { NameString = "Update room" }).ClickAsync();
        // await Page.GotoAsync("http://localhost:5173/room");//5173
        // await Page.PauseAsync();
        await Page.WaitForURLAsync("**/room");
        Assert.That(Page.Url, Does.Contain("/room"));
    }

    [Test]
    public async Task DeleteRoom()
    {
        await Page.GotoAsync("http://localhost:5173/room");//5173
                                                           //await Page.PauseAsync();
        await Page.Locator("td[data-roomNumber='201']").ClickAsync();
        //await Page.PauseAsync();
        var roomNumberField = Page.Locator("input[name='roomNumber']");
        await Expect(roomNumberField).ToBeDisabledAsync();
        await Expect(roomNumberField).ToHaveValueAsync("201");

        var floorField = Page.Locator("input[name='floor']");
        await Expect(floorField).ToBeDisabledAsync();
        await Expect(floorField).ToHaveValueAsync("2");

        var roomTypeField = Page.Locator("select[name='roomTypeID']");
        await Expect(roomTypeField).ToHaveValueAsync("2");
        //await Page.PauseAsync();
        Page.Dialog += async (_, dialog) =>
        {
            await dialog.AcceptAsync(); // Click OK
        };
        await Page.GetByRole(AriaRole.Button, new() { NameString = "Delete room" }).ClickAsync();
        // await Page.GotoAsync("http://localhost:5173/room");//5173
        // await Page.PauseAsync();
        await Page.WaitForURLAsync("**/room");
        Assert.That(Page.Url, Does.Contain("/room"));
    }

    [Test]
    public async Task ExitEditRoom()
    {
        await Page.GotoAsync("http://localhost:5173/room");//5173
        await Page.PauseAsync();
        await Page.Locator("td[data-roomNumber='201']").ClickAsync();

        //await Page.PauseAsync();
        await Page.GetByRole(AriaRole.Button, new() { NameString = "x" }).ClickAsync();
        // Wait for navigation to /room
        await Page.WaitForURLAsync("**/room");
        Assert.That(Page.Url, Does.Contain("/room"));
        //await Page.PauseAsync();
    }

    [Test]
    public async Task ExitAddRoom()
    {
        await Page.GotoAsync("http://localhost:5173/room");//5173
        await Page.PauseAsync();
        await Page.GetByRole(AriaRole.Button, new() { NameString = "Add Room" }).ClickAsync();

        //await Page.PauseAsync();
        await Page.GetByRole(AriaRole.Button, new() { NameString = "x" }).ClickAsync();
        // Wait for navigation to /room
        await Page.WaitForURLAsync("**/room");

        // Assert we are on the /room page
        Assert.That(Page.Url, Does.Contain("/room"));
        await Page.PauseAsync();
    }

    [Test]
    public async Task AddReservation()
    {
        await Page.GotoAsync("http://localhost:5173/reservation");//5173
        await Page.PauseAsync();
        await Page.GetByRole(AriaRole.Button, new() { NameString = "Add Reservation" }).ClickAsync();
        await Page.PauseAsync();

        var totalPriceField = Page.Locator("input[name=\"totalPrice\"]");
        await Expect(totalPriceField).ToHaveValueAsync("0");

        var checkInField = Page.Locator("input[name=\"checkInDate\"]");
        var checkOutField = Page.Locator("input[name=\"checkOutDate\"]");

        await Page.Locator("input[name=\"guestID\"]").ClickAsync();
        await Page.Locator("input[name=\"guestID\"]").FillAsync("1234567890123");
        await Page.Locator("input[name=\"roomNumber\"]").ClickAsync();
        await Page.Locator("input[name=\"roomNumber\"]").FillAsync("101");
        await checkInField.ClickAsync();
        await checkInField.FillAsync("2024-12-20");
        await Expect(checkInField).ToHaveValueAsync("2024-12-20");
        await checkOutField.ClickAsync();
        await checkOutField.FillAsync("2024-12-25");
        await Expect(checkOutField).ToHaveValueAsync("2024-12-25");

        await Expect(totalPriceField).ToHaveValueAsync("250");

        await Page.GetByRole(AriaRole.Button, new() { NameString = "Room Services" }).ClickAsync();
        await Page.PauseAsync();

        var checkboxGroup = Page.Locator(".checkbox-group[data-testid='room-services']");
        await Expect(checkboxGroup).ToBeVisibleAsync();

        var minibarCheckbox = checkboxGroup.Locator("input[type='checkbox'][value='7']"); // replace 7 with actual ID
        await minibarCheckbox.CheckAsync();

        await Expect(totalPriceField).ToHaveValueAsync("255");

        var breakfastCheckbox = checkboxGroup.Locator("input[type='checkbox'][value='1']");
        await breakfastCheckbox.CheckAsync();

        await Expect(totalPriceField).ToHaveValueAsync("265");

        await Expect(minibarCheckbox).ToBeCheckedAsync();
        await Expect(breakfastCheckbox).ToBeCheckedAsync();

        await Page.PauseAsync();


        await Page.GetByRole(AriaRole.Button, new() { NameString = "Extra Services" }).ClickAsync();
        await Page.PauseAsync();

        var checkboxGroup2 = Page.Locator(".checkbox-group[data-testid='extra-services']");
        await Expect(checkboxGroup2).ToBeVisibleAsync();

        var wellnessAccessCheckbox = checkboxGroup2.Locator("input[type='checkbox'][value='3']"); // replace 7 with actual ID
        await wellnessAccessCheckbox.CheckAsync();

        await Expect(totalPriceField).ToHaveValueAsync("415");

        var citytourCheckbox = checkboxGroup2.Locator("input[type='checkbox'][value='5']");
        await citytourCheckbox.CheckAsync();

        await Expect(totalPriceField).ToHaveValueAsync("665");

        await Expect(wellnessAccessCheckbox).ToBeCheckedAsync();
        await Expect(citytourCheckbox).ToBeCheckedAsync();

        await Page.PauseAsync();

        await Page.GetByRole(AriaRole.Button, new() { NameString = "Add reservation" }).ClickAsync();
        await Page.GotoAsync("http://localhost:5173/reservation");//5173
        await Page.PauseAsync();
        await Page.WaitForURLAsync("**/reservation");
        Assert.That(Page.Url, Does.Contain("/reservation"));
    }

    [Test]
    public async Task EditReservation()
    {
        await Page.GotoAsync("http://localhost:5173/reservation");//5173
        await Page.PauseAsync();
        await Page.Locator("div.entity-card[data-reservation-id='1']").ClickAsync();
        await Page.PauseAsync();

        var totalPriceField = Page.Locator("input[name=\"totalPrice\"]");
        await Expect(totalPriceField).ToHaveValueAsync("387");

        var checkInField = Page.Locator("input[name=\"checkInDate\"]");
        var checkOutField = Page.Locator("input[name=\"checkOutDate\"]");
        var guestField = Page.Locator("input[name=\"guestID\"]");
        var roomField = Page.Locator("input[name=\"roomNumber\"]");
        // await checkInField.ClickAsync();
        // await checkInField.FillAsync("2025-01-10");
        await Expect(guestField).ToHaveValueAsync("1234567890123");
        await Expect(roomField).ToHaveValueAsync("101");
        await Expect(checkInField).ToHaveValueAsync("2025-01-10");
        await Expect(checkOutField).ToHaveValueAsync("2025-01-14");

        //await Expect(totalPriceField).ToHaveValueAsync("250");

        await Page.GetByRole(AriaRole.Button, new() { NameString = "Room Services" }).ClickAsync();
        await Page.PauseAsync();
        var checkboxGroup = Page.Locator(".checkbox-group[data-testid='room-services']");
        await Expect(checkboxGroup).ToBeVisibleAsync();

        var breakfastCheckbox = checkboxGroup.Locator("input[type='checkbox'][value='1']");
        var laundryCheckbox = checkboxGroup.Locator("input[type='checkbox'][value='3']");
        var spaCheckbox = checkboxGroup.Locator("input[type='checkbox'][value='5']");

        await Expect(breakfastCheckbox).ToBeCheckedAsync();
        await Expect(laundryCheckbox).ToBeCheckedAsync();
        await Expect(spaCheckbox).ToBeCheckedAsync();

        await Page.PauseAsync();

        await Page.GetByRole(AriaRole.Button, new() { NameString = "Extra Services", Exact = true }).ClickAsync();
        await Page.PauseAsync();
        var checkboxGroup2 = Page.Locator(".checkbox-group[data-testid='extra-services']");
        await Expect(checkboxGroup2).ToBeVisibleAsync();

        var parkingSpotCheckbox = checkboxGroup2.Locator("input[type='checkbox'][value='1']");
        var restaurantCheckbox = checkboxGroup2.Locator("input[type='checkbox'][value='2']");

        await Expect(parkingSpotCheckbox).ToBeCheckedAsync();
        await Expect(restaurantCheckbox).ToBeCheckedAsync();

        await Page.PauseAsync();

        //MENJAM
        guestField.ClickAsync();
        await guestField.FillAsync("3456789012345");
        await Expect(guestField).ToHaveValueAsync("3456789012345");

        checkInField.ClickAsync();
        await checkInField.FillAsync("2025-01-11");//jedan dan manje
        await Expect(checkInField).ToHaveValueAsync("2025-01-11");

        await Expect(totalPriceField).ToHaveValueAsync("302");
        await Page.PauseAsync();

        await breakfastCheckbox.UncheckAsync();
        await Expect(breakfastCheckbox).Not.ToBeCheckedAsync();

        await Expect(totalPriceField).ToHaveValueAsync("292");
        await Page.PauseAsync();

        await parkingSpotCheckbox.UncheckAsync();
        await Expect(parkingSpotCheckbox).Not.ToBeCheckedAsync();

        await Expect(totalPriceField).ToHaveValueAsync("262");
        await Page.PauseAsync();

        await Page.GetByRole(AriaRole.Button, new() { NameString = "Update reservation" }).ClickAsync();
        await Page.GotoAsync("http://localhost:5173/reservation");//5173
        await Page.PauseAsync();
        await Page.WaitForURLAsync("**/reservation");
        Assert.That(Page.Url, Does.Contain("/reservation"));
    }

    [Test]
    public async Task DeleteReservation()
    {
        await Page.GotoAsync("http://localhost:5173/reservation");//5173
        //await Page.PauseAsync();
        await Page.Locator("div.entity-card[data-reservation-id='1']").ClickAsync();
        //await Page.PauseAsync();

        var totalPriceField = Page.Locator("input[name=\"totalPrice\"]");
        await Expect(totalPriceField).ToHaveValueAsync("387");

        var checkInField = Page.Locator("input[name=\"checkInDate\"]");
        var checkOutField = Page.Locator("input[name=\"checkOutDate\"]");
        var guestField = Page.Locator("input[name=\"guestID\"]");
        var roomField = Page.Locator("input[name=\"roomNumber\"]");
        // await checkInField.ClickAsync();
        // await checkInField.FillAsync("2025-01-10");
        await Expect(guestField).ToHaveValueAsync("1234567890123");
        await Expect(roomField).ToHaveValueAsync("101");
        await Expect(checkInField).ToHaveValueAsync("2025-01-10");
        await Expect(checkOutField).ToHaveValueAsync("2025-01-14");

        //await Expect(totalPriceField).ToHaveValueAsync("250");

        await Page.GetByRole(AriaRole.Button, new() { NameString = "Room Services" }).ClickAsync();
        //await Page.PauseAsync();
        var checkboxGroup = Page.Locator(".checkbox-group[data-testid='room-services']");
        await Expect(checkboxGroup).ToBeVisibleAsync();

        var breakfastCheckbox = checkboxGroup.Locator("input[type='checkbox'][value='1']");
        var laundryCheckbox = checkboxGroup.Locator("input[type='checkbox'][value='3']");
        var spaCheckbox = checkboxGroup.Locator("input[type='checkbox'][value='5']");

        await Expect(breakfastCheckbox).ToBeCheckedAsync();
        await Expect(laundryCheckbox).ToBeCheckedAsync();
        await Expect(spaCheckbox).ToBeCheckedAsync();

        //await Page.PauseAsync();

        await Page.GetByRole(AriaRole.Button, new() { NameString = "Extra Services", Exact = true }).ClickAsync();
        //await Page.PauseAsync();
        var checkboxGroup2 = Page.Locator(".checkbox-group[data-testid='extra-services']");
        await Expect(checkboxGroup2).ToBeVisibleAsync();

        var parkingSpotCheckbox = checkboxGroup2.Locator("input[type='checkbox'][value='1']");
        var restaurantCheckbox = checkboxGroup2.Locator("input[type='checkbox'][value='2']");

        await Expect(parkingSpotCheckbox).ToBeCheckedAsync();
        await Expect(restaurantCheckbox).ToBeCheckedAsync();

        //await Page.PauseAsync();

        Page.Dialog += async (_, dialog) =>
        {
            await dialog.AcceptAsync(); // Click OK
        };
        await Page.GetByRole(AriaRole.Button, new() { NameString = "Delete reservation" }).ClickAsync();
        // await Page.GotoAsync("http://localhost:5173/reservation");//5173
        // await Page.PauseAsync();
        await Page.WaitForURLAsync("**/reservation");
        Assert.That(Page.Url, Does.Contain("/reservation"));
    }

    [Test]
    public async Task ExitEditReservation()
    {
        await Page.GotoAsync("http://localhost:5173/reservation");//5173
        await Page.PauseAsync();
        await Page.Locator("div.entity-card[data-reservation-id='1']").ClickAsync();

        //await Page.PauseAsync();
        await Page.GetByRole(AriaRole.Button, new() { NameString = "x", Exact = true }).ClickAsync();
        // Wait for navigation to /reservation
        await Page.WaitForURLAsync("**/reservation");
        Assert.That(Page.Url, Does.Contain("/reservation"));
        //await Page.PauseAsync();
    }

    [Test]
    public async Task ExitAddReservation()
    {
        await Page.GotoAsync("http://localhost:5173/reservation");//5173
        await Page.PauseAsync();
        await Page.GetByRole(AriaRole.Button, new() { NameString = "Add Reservation" }).ClickAsync();

        //await Page.PauseAsync();
        await Page.GetByRole(AriaRole.Button, new() { NameString = "x", Exact = true }).ClickAsync();
        // Wait for navigation to /reservation
        await Page.WaitForURLAsync("**/reservation");

        // Assert we are on the /reservation page
        Assert.That(Page.Url, Does.Contain("/reservation"));
        await Page.PauseAsync();
    }


    [TearDown]
    public async Task TearDown()
    {
        //_context.Database.EnsureDeleted();
        await _context.DisposeAsync();
    }
}
