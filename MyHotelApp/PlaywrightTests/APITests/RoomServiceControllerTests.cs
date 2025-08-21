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
    public async Task GetRoomServices_DisplaysApiRoomServicesInTable()
    {
        await using var response = await Request.GetAsync("/api/RoomService/GetAllRoomServices");

        if (response.Status != 200)
        {
            Assert.Fail($"Code: {response.Status} - {response.StatusText}");
        }

        var jsonResult = await response.JsonAsync();

        if (!jsonResult.HasValue || jsonResult.Value.ValueKind != System.Text.Json.JsonValueKind.Array)
        {
            Assert.Fail("JSON response is not a valid array.");
        }

        var roomServices = jsonResult.Value.EnumerateArray().ToList();

        Assert.That(roomServices.Count, Is.GreaterThan(0), "Expected at least one room service in the response.");

        var firstService = roomServices.First();

        if (!(firstService.TryGetProperty("roomServiceID", out var roomServiceID) &&
            firstService.TryGetProperty("itemName", out var itemName) &&
            firstService.TryGetProperty("itemPrice", out var itemPrice) &&
            firstService.TryGetProperty("description", out var description)))
        {
            Assert.Fail("JSON does not contain expected RoomService properties.");
        }
        else
        {
            Assert.Multiple(() =>
            {
                Assert.That(roomServiceID.GetInt32(), Is.GreaterThan(0), "roomServiceID should be greater than 0.");
                Assert.That(!string.IsNullOrEmpty(itemName.GetString()), "itemName should not be null or empty.");
                ////Console.WriteLine($"Item Name: {itemName.GetString()}");
                Assert.That(itemPrice.GetDecimal(), Is.GreaterThan(0), "itemPrice should be greater than 0.");
                // description može da bude null
                Assert.That(description.ValueKind == System.Text.Json.JsonValueKind.String 
                            || description.ValueKind == System.Text.Json.JsonValueKind.Null,
                            "description should be a string or null.");
            });
        }
    }

    [Test]
    public async Task GetRoomServiceById_ExistingId_ReturnsOkWithCorrectData()
    {
        int serviceId = 1;
        string expectedName = "Breakfast";

        var resp = await Request.GetAsync($"/api/RoomService/GetRoomServiceById/{serviceId}",
            new APIRequestContextOptions { Headers = new Dictionary<string, string> { { "Accept", "application/json" } } });
        var text = await resp.TextAsync();

        //Console.WriteLine($"GET RoomService by ID {serviceId} => Status: {resp.Status}");
        //Console.WriteLine($"Response: {text}");

        Assert.Multiple(() =>
        {
            Assert.That(resp.Status, Is.EqualTo(200));
            Assert.That(text, Does.Contain(expectedName));
        });
    }


    [Test]
    public async Task GetRoomServiceById_NonExistingId_ReturnsNotFound()
    {
        var nonExistingId = 9999;
        var resp = await Request.GetAsync($"/api/RoomService/GetRoomServiceById/{nonExistingId}",
            new APIRequestContextOptions { Headers = new Dictionary<string, string> { { "Accept", "application/json" } } });
        var text = await resp.TextAsync();

        //Console.WriteLine($"GET non-existing RoomService by ID {nonExistingId} => Status: {resp.Status}");
        //Console.WriteLine($"Response: {text}");

        Assert.Multiple(() =>
        {
            Assert.That(resp.Status, Is.EqualTo(404));
            Assert.That(text, Does.Contain($"Room service with ID {nonExistingId} not found"));
        });
    }


    [Test]
    public async Task GetRoomServiceByName_ExistingName_ReturnsOkWithCorrectData()
    {
        string serviceName = "Breakfast";

        var resp = await Request.GetAsync($"/api/RoomService/GetRoomServiceByName/{serviceName}",
            new APIRequestContextOptions { Headers = new Dictionary<string, string> { { "Accept", "application/json" } } });
        var text = await resp.TextAsync();

        //Console.WriteLine($"GET RoomService by Name '{serviceName}' => Status: {resp.Status}");
        //Console.WriteLine($"Response: {text}");

        Assert.Multiple(() =>
        {
            Assert.That(resp.Status, Is.EqualTo(200));
            Assert.That(text, Does.Contain(serviceName));
        });
    }

    [Test]
    public async Task GetRoomServiceByName_NonExistingName_ReturnsNotFound()
    {
        var nonExistingName = "ThisServiceDoesNotExist";
        var resp = await Request.GetAsync($"/api/RoomService/GetRoomServiceByName/{nonExistingName}",
            new APIRequestContextOptions { Headers = new Dictionary<string, string> { { "Accept", "application/json" } } });
        var text = await resp.TextAsync();

        //Console.WriteLine($"GET non-existing RoomService by Name '{nonExistingName}' => Status: {resp.Status}");
        //Console.WriteLine($"Response: {text}");

        Assert.Multiple(() =>
        {
            Assert.That(resp.Status, Is.EqualTo(404));
            Assert.That(text, Does.Contain($"Room service with name {nonExistingName} not found"));
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

        var getResponse = await Request.GetAsync($"/api/RoomService/GetRoomServiceByName/{serviceName}");
        Assert.That(getResponse.Status, Is.EqualTo(200));

        // Deserijalizuj i proveri
        var jsonResult = await getResponse.JsonAsync();
        Assert.That(jsonResult.HasValue, Is.True);

        Assert.That(jsonResult.Value.TryGetProperty("itemName", out var itemName) && itemName.GetString() == serviceName, Is.True);
        Assert.That(jsonResult.Value.TryGetProperty("itemPrice", out var itemPrice) && itemPrice.GetDecimal() == 100, Is.True);
        Assert.That(jsonResult.Value.TryGetProperty("description", out var description) && description.GetString() == "Test description", Is.True);
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

        var body = await resp.BodyAsync();
        var jsonString = Encoding.UTF8.GetString(body);

        //Console.WriteLine($"POST EmptyName => Status: {resp.Status}");
        //Console.WriteLine($"Response: {jsonString}");

        Assert.Multiple(() =>
        {
            Assert.That(resp, Has.Property("Status").EqualTo(400));
            Assert.That(jsonString, Does.Contain("Service name is required and cannot exceed 50 characters"));
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

        var body = await resp.BodyAsync();
        var jsonString = Encoding.UTF8.GetString(body);

        //Console.WriteLine($"POST NameTooLong => Status: {resp.Status}");
        //Console.WriteLine($"Response: {jsonString}");

        Assert.Multiple(() =>
        {
            Assert.That(resp, Has.Property("Status").EqualTo(400));
            Assert.That(jsonString, Does.Contain("Service name is required and cannot exceed 50 characters"));
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

        var body = await resp.BodyAsync();
        var jsonString = Encoding.UTF8.GetString(body);

        //Console.WriteLine($"POST NegativePrice => Status: {resp.Status}");
        //Console.WriteLine($"Response: {jsonString}");

        Assert.Multiple(() =>
        {
            Assert.That(resp, Has.Property("Status").EqualTo(400));
            Assert.That(jsonString, Does.Contain("Price must be a positive value"));
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

        var body = await resp.BodyAsync();
        var jsonString = Encoding.UTF8.GetString(body);

        //Console.WriteLine($"POST ZeroPrice => Status: {resp.Status}");
        //Console.WriteLine($"Response: {jsonString}");

        Assert.Multiple(() =>
        {
            Assert.That(resp, Has.Property("Status").EqualTo(400));
            Assert.That(jsonString, Does.Contain("Price must be a positive value"));
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

        Assert.That(createResp.Status, Is.EqualTo(404));
    }



    [Test]
    public async Task UpdateRoomService_ById_UpdatesSuccessfully()
    {
        var service = await _context.RoomServices.FirstAsync(rs => rs.ItemName == "Breakfast");
        var updatedName = "Breakfast Deluxe";

        var updateResp = await Request.PutAsync($"/api/RoomService/UpdateRoomService/{service.RoomServiceID}", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            DataObject = new
            {
                ItemName = updatedName,
                ItemPrice = 15.00m,
                Description = "Updated description"
            }
        });

        Assert.That(updateResp.Status, Is.EqualTo(200));
        //Console.WriteLine($"Update response status: {updateResp.Status}");

        var getUpdated = await Request.GetAsync($"/api/RoomService/GetRoomServiceById/{service.RoomServiceID}");
        var json = await getUpdated.JsonAsync();
        Assert.That(json.HasValue, Is.True);

        //Console.WriteLine($"Updated JSON: {json.Value}");

        Assert.Multiple(() =>
        {
            Assert.That(json.Value.TryGetProperty("roomServiceID", out var idProp) && idProp.GetInt32() == service.RoomServiceID, "roomServiceID mismatch");
            Assert.That(json.Value.TryGetProperty("itemName", out var nameProp) && nameProp.GetString() == updatedName, "itemName mismatch");
            Assert.That(json.Value.TryGetProperty("itemPrice", out var priceProp) && priceProp.GetDecimal() == 15.00m, "itemPrice mismatch");
            Assert.That(json.Value.TryGetProperty("description", out var descProp) && descProp.GetString() == "Updated description", "description mismatch");
        });
    }

    [Test]
    public async Task UpdateRoomServiceByName_ValidUpdate_Succeeds()
    {
        var serviceName = "Breakfast";
        var updatedName = "Breakfast Deluxe";

        var resp = await Request.PutAsync($"/api/RoomService/UpdateRoomServiceByName/{serviceName}", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            DataObject = new
            {
                ItemName = updatedName,
                ItemPrice = 20m,
                Description = "Updated description"
            }
        });

        var body = await resp.BodyAsync();
        var jsonString = Encoding.UTF8.GetString(body);

        //Console.WriteLine($"PUT ValidUpdate => Status: {resp.Status}");
        //Console.WriteLine($"Response: {jsonString}");

        Assert.Multiple(() =>
        {
            Assert.That(resp, Has.Property("Status").EqualTo(200));
            Assert.That(jsonString, Does.Contain($"Room service with name {serviceName} updated successfully"));
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

        var body = await resp.BodyAsync();
        var jsonString = Encoding.UTF8.GetString(body);

        //Console.WriteLine($"PUT EmptyName => Status: {resp.Status}");
        //Console.WriteLine($"Response: {jsonString}");

        Assert.Multiple(() =>
        {
            Assert.That(resp, Has.Property("Status").EqualTo(400));
            Assert.That(jsonString, Does.Contain("Service name is required and cannot exceed 50 characters"));
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

        var body = await resp.BodyAsync();
        var jsonString = Encoding.UTF8.GetString(body);

        //Console.WriteLine($"PUT NameTooLong => Status: {resp.Status}");
        //Console.WriteLine($"Response: {jsonString}");

        Assert.Multiple(() =>
        {
            Assert.That(resp, Has.Property("Status").EqualTo(400));
            Assert.That(jsonString, Does.Contain("Service name is required and cannot exceed 50 characters"));
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

        var body = await resp.BodyAsync();
        var jsonString = Encoding.UTF8.GetString(body);

        //Console.WriteLine($"PUT NegativePrice => Status: {resp.Status}");
        //Console.WriteLine($"Response: {jsonString}");

        Assert.Multiple(() =>
        {
            Assert.That(resp, Has.Property("Status").EqualTo(400));
            Assert.That(jsonString, Does.Contain("Price must be a positive value"));
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

        var body = await resp.BodyAsync();
        var jsonString = Encoding.UTF8.GetString(body);

        //Console.WriteLine($"PUT ZeroPrice => Status: {resp.Status}");
        //Console.WriteLine($"Response: {jsonString}");

        Assert.Multiple(() =>
        {
            Assert.That(resp, Has.Property("Status").EqualTo(400));
            Assert.That(jsonString, Does.Contain("Price must be a positive value"));
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

        var body = await resp.BodyAsync();
        var jsonString = Encoding.UTF8.GetString(body);

        //Console.WriteLine($"PUT DuplicateName => Status: {resp.Status}");
        //Console.WriteLine($"Response: {jsonString}");

        Assert.Multiple(() =>
        {
            Assert.That(resp, Has.Property("Status").EqualTo(400));
            Assert.That(jsonString, Does.Contain($"Room service with the name {existingName} already exists"));
        });
    }


    [Test]
    public async Task DeleteRoomService_ById_DeletesSuccessfully()
    {
        int serviceId = 1; 

        var deleteResp = await Request.DeleteAsync($"/api/RoomService/DeleteRoomService/{serviceId}");
        //Console.WriteLine($"Delete response status: {deleteResp.Status}");

        var getDeleted = await Request.GetAsync($"/api/RoomService/GetRoomServiceById/{serviceId}");
        //Console.WriteLine($"GET after delete status: {getDeleted.Status}");

        Assert.Multiple(() =>
        {
            Assert.That(deleteResp.Status, Is.EqualTo(200));
            Assert.That(getDeleted.Status, Is.EqualTo(404));
        });
    }

    [Test]
    public async Task DeleteRoomService_ById_NonExistingId_ReturnsNotFound()
    {
        int nonExistingId = 9999;

        var deleteResp = await Request.DeleteAsync($"/api/RoomService/DeleteRoomService/{nonExistingId}");
        var text = await deleteResp.TextAsync();
        //Console.WriteLine($"Delete non-existing ID response status: {deleteResp.Status}");
        //Console.WriteLine($"Response text: {text}");

        Assert.Multiple(() =>
        {
            Assert.That(deleteResp.Status, Is.EqualTo(404));
            Assert.That(text, Does.Contain($"Room service with ID {nonExistingId} not found"));
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

