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
public class RoomControllerTests : PlaywrightTest
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
    public async Task CreateRoom_ValidInput_ReturnsOkAndCreatesRoom()
    {
        int newRoomNumber = 350; 
        
        await using var response = await Request.PostAsync("/api/Room/CreateRoom", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"}
            },
            DataObject = new
            {
                RoomNumber = newRoomNumber,
                RoomTypeID = 1,
                Floor = 3
            }
        });

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(200));
            Assert.That(responseText, Does.Contain($"Room with number {newRoomNumber} created successfully."));
        });

    }

    [Test]
    public async Task CreateRoom_DuplicateRoomNumber_ReturnsBadRequest()
    {
        int existingRoomNumber = 101;

        await using var response = await Request.PostAsync("/api/Room/CreateRoom", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"}
            },
            DataObject = new { RoomNumber = existingRoomNumber, RoomTypeID = 1, Floor = 1 }
        });

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(400));
            Assert.That(responseText, Does.Contain($"Room with number {existingRoomNumber} already exists."));
        });
    }

    [Test]
    public async Task CreateRoom_InvalidRoomNumber_ReturnsNotFound()
    {
        int invalidRoomNumber = 700; // van opsega 101-699

        await using var response = await Request.PostAsync("/api/Room/CreateRoom", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"}
            },
            DataObject = new
            {
                RoomNumber = invalidRoomNumber,
                RoomTypeID = 1,
                Floor = 3
            }
        });

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(400));
            Assert.That(responseText, Does.Contain("Room number must be between 101 and 699."));
        });
    }

    [Test]
    public async Task CreateRoom_InvalidRoomType_ReturnsNotFound()
    {
        int newRoomNumber = 350;

        await using var response = await Request.PostAsync("/api/Room/CreateRoom", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"}
            },
            DataObject = new
            {
                RoomNumber = newRoomNumber,
                RoomTypeID = 9999,
                Floor = 3
            }
        });

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(404));
            Assert.That(responseText, Does.Contain("Room type with ID 9999 does not exist."));
        });
    }

    [Test]
    public async Task GetAllRooms_ReturnsOk()
    {
        await using var response = await Request.GetAsync("/api/Room/GetAllRooms");

        if (response.Status != 200)
        {
            Assert.Fail($"Code: {response.Status} - {response.StatusText}");
        }

        var jsonResult = await response.JsonAsync();

        Assert.Multiple(() =>
        {
            Assert.That(jsonResult.HasValue, Is.True);
            Assert.That(jsonResult.Value.ValueKind, Is.EqualTo(JsonValueKind.Array));
        });

        var rooms = jsonResult.Value.EnumerateArray().ToList();

        Assert.That(rooms.Count, Is.GreaterThan(0), "Expected at least one room in the response.");
    }

    [Test]
    public async Task GetRoom_ValidRoomNumber_ReturnsOkAndRoom()
    {
        int validRoomNumber = 101;

        await using var response = await Request.GetAsync($"/api/Room/GetRoom/{validRoomNumber}");

        if (response.Status != 200)
        {
            Assert.Fail($"Code: {response.Status} - {response.StatusText}");
        }

        var jsonResult = await response.JsonAsync();

        Assert.Multiple(() =>
        {
            Assert.That(jsonResult.HasValue, Is.True, "Response did not contain JSON.");
            Assert.That(jsonResult.Value.ValueKind, Is.EqualTo(JsonValueKind.Object), "Expected JSON object but got something else.");
        });

        jsonResult.Value.TryGetProperty("roomNumber", out var roomNumber);
        jsonResult.Value.TryGetProperty("roomTypeID", out var roomTypeID);
        jsonResult.Value.TryGetProperty("floor", out var floor);

        Assert.Multiple(() =>
        {
            Assert.That(roomNumber.GetInt32(), Is.EqualTo(validRoomNumber));
            Assert.That(roomTypeID.GetInt32(), Is.EqualTo(1));
            Assert.That(floor.GetInt32(), Is.EqualTo(1)); 
        });
    }

    [Test]
    public async Task GetRoomById_NonExisting_ReturnsNotFound()
    {
        int roomNumber = 999;
        await using var response = await Request.GetAsync($"/api/Room/GetRoom/{roomNumber}");

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(404));
            Assert.That(responseText, Does.Contain($"Room with number {roomNumber} not found."));
        });
    }

    [Test]
    public async Task UpdateRoom_ValidInput_ReturnsOkAndUpdatesRoom()
    {
        int existingRoomNumber = 101;

        // Update sobe
        await using var response = await Request.PutAsync($"/api/Room/UpdateRoom/{existingRoomNumber}", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"}
            },
            DataObject = new
            {
                RoomNumber = existingRoomNumber,
                RoomTypeID = 2, 
                Floor = 1 
            }
        });

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(200));
            Assert.That(responseText, Does.Contain($"Room with number {existingRoomNumber} updated successfully."));
        });

    }


    [Test]
    public async Task UpdateRoom_NonExisting_ReturnsNotFound()
    {
        int nonExistingRoomNumber = 8888;

        await using var response = await Request.PutAsync($"/api/Room/UpdateRoom/{nonExistingRoomNumber}", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"}
            },
            DataObject = new { RoomNumber = nonExistingRoomNumber, RoomTypeID = 1, Floor = 1 }
        });

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(404));
            Assert.That(responseText, Does.Contain($"Room with number {nonExistingRoomNumber} not found."));
        });
    }

    [Test]
    public async Task UpdateRoom_InvalidRoomTypeID_ReturnsNotFound()
    {
        int existingRoomNumber = 101; 

        await using var response = await Request.PutAsync($"/api/Room/UpdateRoom/{existingRoomNumber}", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"}
            },
            DataObject = new
            {
                RoomNumber = existingRoomNumber,
                RoomTypeID = 9999, 
                Floor = 3
            }
        });

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(404));
            Assert.That(responseText, Does.Contain("Room type with ID 9999 does not exist."));
        });
    }

    [Test]
    public async Task DeleteRoom_Existing_ReturnsOk()
    {
        int existingRoomNumber = 101;

        await using var response = await Request.DeleteAsync($"/api/Room/DeleteRoom/{existingRoomNumber}");
        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(200));
            Assert.That(responseText, Does.Contain($"Room with number {existingRoomNumber} deleted successfully."));
        });
    }

    [Test]
    public async Task DeleteRoom_NonExistingRoomNumber_ReturnsNotFound()
    {
        int nonExistingRoomNumber = 999;

        await using var response = await Request.DeleteAsync($"/api/Room/DeleteRoom/{nonExistingRoomNumber}");
        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(404));
            Assert.That(responseText, Does.Contain($"Room with number {nonExistingRoomNumber} not found."));
        });
    }


    [TearDown]
    public async Task TearDownAPITesting()
    {
        await Request.DisposeAsync();
        await _context.DisposeAsync();
    }
}