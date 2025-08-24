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
public class RoomServiceControllerTests : PlaywrightTest
{
    private IAPIRequestContext Request;
    private HotelContext _context;
    //var page = await context.NewPageAsync();


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
    public async Task GetAllRoomServices_ReturnsOkAndRoomServices()
    {
        await using var response = await Request.GetAsync("/api/RoomService/GetAllRoomServices");

        if (response.Status != 200)
        {
            Assert.Fail($"Code: {response.Status} - {response.StatusText}");
        }

        var jsonResult = await response.JsonAsync();

        Assert.Multiple(() =>
        {
            Assert.That(jsonResult.HasValue, Is.True, "Response did not contain JSON.");
            Assert.That(jsonResult.Value.ValueKind, Is.EqualTo(JsonValueKind.Array), "Expected JSON array.");
        });

        var roomServices = jsonResult.Value.EnumerateArray().ToList();

        Assert.That(roomServices.Count, Is.GreaterThan(0), "Expected at least one room service in the response.");

    }


    [Test]
    public async Task GetRoomServiceById_ExistingId_ReturnsOkAndRoomService()
    {
        int serviceId = 1;
        string expectedName = "Breakfast";

        await using var response = await Request.GetAsync($"/api/RoomService/GetRoomServiceById/{serviceId}");

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

        jsonResult.Value.TryGetProperty("roomServiceID", out var id);
        jsonResult.Value.TryGetProperty("itemName", out var name);
        jsonResult.Value.TryGetProperty("itemPrice", out var price);
        jsonResult.Value.TryGetProperty("description", out var description);

        Assert.Multiple(() =>
        {
            Assert.That(id.GetInt32(), Is.EqualTo(serviceId));
            Assert.That(name.GetString(), Is.EqualTo(expectedName));
            Assert.That(price.GetDecimal(), Is.GreaterThan(0));
            Assert.That(description.ValueKind == JsonValueKind.String || description.ValueKind == JsonValueKind.Null);
        });
    }

    [Test]
    public async Task GetRoomServiceById_NonExistingId_ReturnsNotFound()
    {
        int nonExistingId = 9999;

        await using var response = await Request.GetAsync($"/api/RoomService/GetRoomServiceById/{nonExistingId}");
        var text = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.Status, Is.EqualTo(404));
            Assert.That(text, Does.Contain($"Room service with ID {nonExistingId} not found."));
        });
    }

    [Test]
    public async Task GetRoomServiceByName_ExistingName_ReturnsOkAndRoomService()
    {
        string serviceName = "Breakfast";

        await using var response = await Request.GetAsync($"/api/RoomService/GetRoomServiceByName/{serviceName}");

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

        jsonResult.Value.TryGetProperty("roomServiceID", out var id);
        jsonResult.Value.TryGetProperty("itemName", out var name);
        jsonResult.Value.TryGetProperty("itemPrice", out var price);
        jsonResult.Value.TryGetProperty("description", out var description);

        Assert.Multiple(() =>
        {
            Assert.That(name.GetString(), Is.EqualTo(serviceName));
            Assert.That(id.GetInt32(), Is.GreaterThan(0));
            Assert.That(price.GetDecimal(), Is.GreaterThan(0));
            Assert.That(description.ValueKind == JsonValueKind.String || description.ValueKind == JsonValueKind.Null);
        });
    }

    [Test]
    public async Task GetRoomServiceByName_NonExistingName_ReturnsNotFound()
    {
        string nonExistingName = "ThisServiceDoesNotExist";

        await using var response = await Request.GetAsync($"/api/RoomService/GetRoomServiceByName/{nonExistingName}");
        var text = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.Status, Is.EqualTo(404));
            Assert.That(text, Does.Contain($"Room service with name {nonExistingName} not found."));
        });
    }



    [Test]
    public async Task CreateRoomService_ValidData_CheckDatabase()
    {
        var serviceName = "TestService";

        var createResponse = await Request.PostAsync("/api/RoomService/CreateRoomService", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            DataObject = new
            {
                ItemName = serviceName,
                ItemPrice = 100,
                Description = "Test description"
            }
        });

        Assert.That(createResponse.Status, Is.EqualTo(200));
        var createText = await createResponse.TextAsync();
        Assert.That(createText, Is.EqualTo($"\"Room service item with name {serviceName} created successfully.\""));
    }

    [Test]
    public async Task CreateRoomService_EmptyName_ReturnsBadRequest()
    {
        var resp = await Request.PostAsync("/api/RoomService/CreateRoomService", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            DataObject = new
            {
                ItemName = "",
                ItemPrice = 10m,
                Description = "Empty name test"
            }
        });

        
        var responseText = await resp.TextAsync();;

        //Console.WriteLine($"POST EmptyName => Status: {resp.Status}");
        //Console.WriteLine($"Response: {jsonString}");

        Assert.Multiple(() =>
        {
            Assert.That(resp, Has.Property("Status").EqualTo(400));
            Assert.That(responseText, Does.Contain("Service name is required and cannot exceed 50 characters"));
        });
    }

    [Test]
    public async Task CreateRoomService_NameTooLong_ReturnsBadRequest()
    {
        var resp = await Request.PostAsync("/api/RoomService/CreateRoomService", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            DataObject = new
            {
                ItemName = new string('a', 51),
                ItemPrice = 10m,
                Description = "Name too long test"
            }
        });

        var responseText = await resp.TextAsync();


        Assert.Multiple(() =>
        {
            Assert.That(resp, Has.Property("Status").EqualTo(400));
            Assert.That(responseText, Does.Contain("Service name is required and cannot exceed 50 characters"));
        });
    }

    [Test]
    public async Task CreateRoomService_NegativePrice_ReturnsBadRequest()
    {
        var resp = await Request.PostAsync("/api/RoomService/CreateRoomService", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            DataObject = new
            {
                ItemName = "NegativePrice",
                ItemPrice = -5m,
                Description = "Negative price test"
            }
        });

        var responseText = await resp.TextAsync();

        //Console.WriteLine($"POST NegativePrice => Status: {resp.Status}");
        //Console.WriteLine($"Response: {jsonString}");

        Assert.Multiple(() =>
        {
            Assert.That(resp, Has.Property("Status").EqualTo(400));
            Assert.That(responseText, Does.Contain("Price must be a positive value"));
        });
    }

    [Test]
    public async Task CreateRoomService_ZeroPrice_ReturnsBadRequest()
    {
        var resp = await Request.PostAsync("/api/RoomService/CreateRoomService", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            DataObject = new
            {
                ItemName = "ZeroPrice",
                ItemPrice = 0m,
                Description = "Zero price test"
            }
        });

        var responseText = await resp.TextAsync();

        //Console.WriteLine($"POST ZeroPrice => Status: {resp.Status}");
        //Console.WriteLine($"Response: {jsonString}");

        Assert.Multiple(() =>
        {
            Assert.That(resp, Has.Property("Status").EqualTo(400));
            Assert.That(responseText, Does.Contain("Price must be a positive value"));
        });
    }

    [Test]
    public async Task CreateRoomService_DuplicateName_ReturnsError()
    {
        var existingName = "Breakfast"; 
        var createResp = await Request.PostAsync("/api/RoomService/CreateRoomService", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            DataObject = new { ItemName = existingName, ItemPrice = 100.0m, Description = "Duplicate Test" }
        });

        //Console.WriteLine($"Create duplicate response status: {createResp.Status}");
        var text = await createResp.TextAsync();
        //Console.WriteLine($"Response text: {text}");

        Assert.Multiple(() =>
        {
            Assert.That(createResp, Has.Property("Status").EqualTo(404));
            Assert.That(text, Does.Contain($"Room service with the name {existingName} already exists."));
        });
    }



    [Test]
    public async Task UpdateRoomService_ById_ReturnsOkAndUpdatesRoomService()
    {
        int existingServiceId = 1; 

        await using var response = await Request.PutAsync($"/api/RoomService/UpdateRoomService/{existingServiceId}", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            },
            DataObject = new
            {
                ItemName = "Breakfast Deluxe",
                ItemPrice = 15.00m,
                Description = "Updated description"
            }
        });

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(200));
            Assert.That(responseText, Does.Contain($"Room service with ID {existingServiceId} updated successfully."));
        });
    }


    [Test]
    public async Task UpdateRoomServiceByName_ValidUpdate_ReturnsOk()
    {
        string existingServiceName = "Breakfast";
        await using var response = await Request.PutAsync($"/api/RoomService/UpdateRoomServiceByName/{existingServiceName}", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            },
            DataObject = new
            {
                ItemName = "Breakfast Deluxe",
                ItemPrice = 20.00m,
                Description = "Updated description"
            }
        });

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(200));
            Assert.That(responseText, Does.Contain($"Room service with name {existingServiceName} updated successfully."));
        });
    }

    [Test]
    public async Task UpdateRoomServiceByName_EmptyName_ReturnsBadRequest()
    {
        var serviceName = "Breakfast";

        var resp = await Request.PutAsync($"/api/RoomService/UpdateRoomServiceByName/{serviceName}", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            DataObject = new
            {
                ItemName = "",
                ItemPrice = 20m,
                Description = "Empty name test"
            }
        });

        var responseText = await resp.TextAsync();

        //Console.WriteLine($"PUT EmptyName => Status: {resp.Status}");
        //Console.WriteLine($"Response: {jsonString}");

        Assert.Multiple(() =>
        {
            Assert.That(resp, Has.Property("Status").EqualTo(400));
            Assert.That(responseText, Does.Contain("Service name is required and cannot exceed 50 characters"));
        });
    }

    [Test]
    public async Task UpdateRoomServiceByName_NameTooLong_ReturnsBadRequest()
    {
        var serviceName = "Breakfast";

        var resp = await Request.PutAsync($"/api/RoomService/UpdateRoomServiceByName/{serviceName}", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            DataObject = new
            {
                ItemName = new string('a', 51),
                ItemPrice = 20m,
                Description = "Name too long test"
            }
        });

        
        var responseText = await resp.TextAsync();

        //Console.WriteLine($"PUT NameTooLong => Status: {resp.Status}");
        //Console.WriteLine($"Response: {jsonString}");

        Assert.Multiple(() =>
        {
            Assert.That(resp, Has.Property("Status").EqualTo(400));
            Assert.That(responseText, Does.Contain("Service name is required and cannot exceed 50 characters"));
        });
    }

    [Test]
    public async Task UpdateRoomServiceByName_NegativePrice_ReturnsBadRequest()
    {
        var serviceName = "Breakfast";

        var resp = await Request.PutAsync($"/api/RoomService/UpdateRoomServiceByName/{serviceName}", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            DataObject = new
            {
                ItemName = "Breakfast Updated",
                ItemPrice = -5m,
                Description = "Negative price test"
            }
        });

        var responseText = await resp.TextAsync();

        //Console.WriteLine($"PUT NegativePrice => Status: {resp.Status}");
        //Console.WriteLine($"Response: {jsonString}");

        Assert.Multiple(() =>
        {
            Assert.That(resp, Has.Property("Status").EqualTo(400));
            Assert.That(responseText, Does.Contain("Price must be a positive value"));
        });
    }

    [Test]
    public async Task UpdateRoomServiceByName_ZeroPrice_ReturnsBadRequest()
    {
        var serviceName = "Breakfast";

        var resp = await Request.PutAsync($"/api/RoomService/UpdateRoomServiceByName/{serviceName}", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            DataObject = new
            {
                ItemName = "Breakfast Updated",
                ItemPrice = 0m,
                Description = "Zero price test"
            }
        });

        var responseText = await resp.TextAsync();

        //Console.WriteLine($"PUT ZeroPrice => Status: {resp.Status}");
        //Console.WriteLine($"Response: {jsonString}");

        Assert.Multiple(() =>
        {
            Assert.That(resp, Has.Property("Status").EqualTo(400));
            Assert.That(responseText, Does.Contain("Price must be a positive value"));
        });
    }

    [Test]
    public async Task UpdateRoomServiceByName_DuplicateName_ReturnsBadRequest()
    {
        var serviceName = "Breakfast";
        var existingName = "Gym Access"; 

        var resp = await Request.PutAsync($"/api/RoomService/UpdateRoomServiceByName/{serviceName}", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            DataObject = new
            {
                ItemName = existingName,
                ItemPrice = 25m,
                Description = "Duplicate name test"
            }
        });

        var responseText = await resp.TextAsync();

        //Console.WriteLine($"PUT DuplicateName => Status: {resp.Status}");
        //Console.WriteLine($"Response: {jsonString}");

        Assert.Multiple(() =>
        {
            Assert.That(resp, Has.Property("Status").EqualTo(400));
            Assert.That(responseText, Does.Contain($"Room service with the name {existingName} already exists"));
        });
    }


    [Test]
    public async Task DeleteRoomService_ExistingId_ReturnsOk()
    {
        int existingServiceId = 1;

        await using var response = await Request.DeleteAsync($"/api/RoomService/DeleteRoomService/{existingServiceId}");
        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(200));
            Assert.That(responseText, Does.Contain($"Room service with ID {existingServiceId} deleted successfully."));
        });
    }

    [Test]
    public async Task DeleteRoomService_NonExistingId_ReturnsNotFound()
    {
        int nonExistingServiceId = 9999;

        await using var response = await Request.DeleteAsync($"/api/RoomService/DeleteRoomService/{nonExistingServiceId}");
        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(404));
            Assert.That(responseText, Does.Contain($"Room service with ID {nonExistingServiceId} not found."));
        });
    }


    [Test]
    public async Task DeleteRoomService_ById_Twice_SecondTimeReturnsNotFound()
    {
        int serviceId = 2;

        var firstDelete = await Request.DeleteAsync($"/api/RoomService/DeleteRoomService/{serviceId}");
        //Console.WriteLine($"First delete response status: {firstDelete.Status}");

        var secondDelete = await Request.DeleteAsync($"/api/RoomService/DeleteRoomService/{serviceId}");
        var text = await secondDelete.TextAsync();
        //Console.WriteLine($"Second delete response status: {secondDelete.Status}");
        //Console.WriteLine($"Response text: {text}");

        Assert.Multiple(() =>
        {
            Assert.That(firstDelete.Status, Is.EqualTo(200));
            Assert.That(secondDelete.Status, Is.EqualTo(404));
            Assert.That(text, Does.Contain($"Room service with ID {serviceId} not found"));
        });
    }

    [TearDown]
    public async Task TearDownAPITesting()
    {
        await Request.DisposeAsync();
        await _context.DisposeAsync();
    }
}

