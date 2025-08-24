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
public class ExtraServiceControllerTests : PlaywrightTest
{
    private IAPIRequestContext Request;
    private HotelContext _context;


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

        await DatabaseRefresher.AddDataAsync(_context);
    }

    [Test]
    public async Task GetAllExtraServices_ReturnsOkAndExtraServices()
    {
        await using var response = await Request.GetAsync("/api/ExtraService/GetAllExtraServices");

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

        var ExtraServices = jsonResult.Value.EnumerateArray().ToList();

        Assert.That(ExtraServices.Count, Is.GreaterThan(0), "Expected at least one Extra service in the response.");

    }


    [Test]
    public async Task GetExtraServiceById_ExistingId_ReturnsOkAndExtraService()
    {
        int serviceId = 1;
        string expectedName = "Parking Spot";

        await using var response = await Request.GetAsync($"/api/ExtraService/GetExtraServiceById/{serviceId}");

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

        jsonResult.Value.TryGetProperty("extraServiceID", out var id);
        jsonResult.Value.TryGetProperty("serviceName", out var name);
        jsonResult.Value.TryGetProperty("price", out var price);
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
    public async Task GetExtraServiceById_NonExistingId_ReturnsNotFound()
    {
        int nonExistingId = 9999;

        await using var response = await Request.GetAsync($"/api/ExtraService/GetExtraServiceById/{nonExistingId}");
        var text = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.Status, Is.EqualTo(404));
            Assert.That(text, Does.Contain($"Extra service with ID {nonExistingId} not found."));
        });
    }

    [Test]
    public async Task GetExtraServiceByName_ExistingName_ReturnsOkAndExtraService()
    {
        string serviceName = "Parking Spot";

        await using var response = await Request.GetAsync($"/api/ExtraService/GetExtraServiceByName/{serviceName}");

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

        jsonResult.Value.TryGetProperty("extraServiceID", out var id);
        jsonResult.Value.TryGetProperty("serviceName", out var name);
        jsonResult.Value.TryGetProperty("price", out var price);
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
    public async Task GetExtraServiceByName_NonExistingName_ReturnsNotFound()
    {
        string nonExistingName = "ThisServiceDoesNotExist";

        await using var response = await Request.GetAsync($"/api/ExtraService/GetExtraServiceByName/{nonExistingName}");
        var text = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.Status, Is.EqualTo(404));
            Assert.That(text, Does.Contain($"Extra service with name {nonExistingName} not found."));
        });
    }



    [Test]
    public async Task CreateExtraService_ValidData_CheckDatabase()
    {
        var serviceName = "TestService";

        var createResponse = await Request.PostAsync("/api/ExtraService/CreateExtraService", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            DataObject = new
            {
                serviceName = serviceName,
                price = 100,
                Description = "Test description"
            }
        });

        Assert.That(createResponse.Status, Is.EqualTo(200));
        var createText = await createResponse.TextAsync();
        Assert.That(createText, Is.EqualTo($"\"Extra service with name {serviceName} created successfully.\""));
    }

    [Test]
    public async Task CreateExtraService_EmptyName_ReturnsBadRequest()
    {
        var resp = await Request.PostAsync("/api/ExtraService/CreateExtraService", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            DataObject = new
            {
                serviceName = "",
                price = 10m,
                Description = "Empty name test"
            }
        });

        
        var responseText = await resp.TextAsync();;

        Assert.Multiple(() =>
        {
            Assert.That(resp, Has.Property("Status").EqualTo(400));
            Assert.That(responseText, Does.Contain("Service name is required and cannot exceed 50 characters"));
        });
    }

    [Test]
    public async Task CreateExtraService_NameTooLong_ReturnsBadRequest()
    {
        var resp = await Request.PostAsync("/api/ExtraService/CreateExtraService", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            DataObject = new
            {
                serviceName = new string('a', 51),
                price = 10m,
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
    public async Task CreateExtraService_Negativeprice_ReturnsBadRequest()
    {
        var resp = await Request.PostAsync("/api/ExtraService/CreateExtraService", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            DataObject = new
            {
                serviceName = "Negativeprice",
                price = -5m,
                Description = "Negative price test"
            }
        });

        var responseText = await resp.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(resp, Has.Property("Status").EqualTo(400));
            Assert.That(responseText, Does.Contain("Price must be a positive value"));
        });
    }

    [Test]
    public async Task CreateExtraService_Zeroprice_ReturnsBadRequest()
    {
        var resp = await Request.PostAsync("/api/ExtraService/CreateExtraService", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            DataObject = new
            {
                serviceName = "Zeroprice",
                price = 0m,
                Description = "Zero price test"
            }
        });

        var responseText = await resp.TextAsync();


        Assert.Multiple(() =>
        {
            Assert.That(resp, Has.Property("Status").EqualTo(400));
            Assert.That(responseText, Does.Contain("Price must be a positive value"));
        });
    }

    [Test]
    public async Task CreateExtraService_DuplicateName_ReturnsError()
    {
        var existingName = "Parking Spot"; 
        var createResp = await Request.PostAsync("/api/ExtraService/CreateExtraService", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            DataObject = new { serviceName = existingName, price = 100.0m, Description = "Duplicate Test" }
        });

        var text = await createResp.TextAsync();
        
        Assert.Multiple(() =>
        {
            Assert.That(createResp, Has.Property("Status").EqualTo(400));
            Assert.That(text, Does.Contain($"Extra service with the name {existingName} already exists."));
        });
    }



    [Test]
    public async Task UpdateExtraService_ById_ReturnsOkAndUpdatesExtraService()
    {
        int existingServiceId = 1; 

        await using var response = await Request.PutAsync($"/api/ExtraService/UpdateExtraService/{existingServiceId}", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            },
            DataObject = new
            {
                serviceName = "Parking Spot Deluxe",
                price = 15.00m,
                Description = "Updated description"
            }
        });

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(200));
            Assert.That(responseText, Does.Contain($"Extra service with ID {existingServiceId} updated successfully."));
        });
    }


    [Test]
    public async Task UpdateExtraServiceByName_ValidUpdate_ReturnsOk()
    {
        string existingServiceName = "Parking Spot";
        await using var response = await Request.PutAsync($"/api/ExtraService/UpdateExtraServiceByName/{existingServiceName}", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            },
            DataObject = new
            {
                serviceName = "Parking Spot Deluxe",
                price = 20.00m,
                Description = "Updated description"
            }
        });

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(200));
            Assert.That(responseText, Does.Contain($"Extra service with name {existingServiceName} updated successfully."));
        });
    }

    [Test]
    public async Task UpdateExtraServiceByName_EmptyName_ReturnsBadRequest()
    {
        var serviceName = "Parking Spot";

        var resp = await Request.PutAsync($"/api/ExtraService/UpdateExtraServiceByName/{serviceName}", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            DataObject = new
            {
                serviceName = "",
                price = 20m,
                Description = "Empty name test"
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
    public async Task UpdateExtraServiceByName_NameTooLong_ReturnsBadRequest()
    {
        var serviceName = "Parking Spot";

        var resp = await Request.PutAsync($"/api/ExtraService/UpdateExtraServiceByName/{serviceName}", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            DataObject = new
            {
                serviceName = new string('a', 51),
                price = 20m,
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
    public async Task UpdateExtraServiceByName_Negativeprice_ReturnsBadRequest()
    {
        var serviceName = "Parking Spot";

        var resp = await Request.PutAsync($"/api/ExtraService/UpdateExtraServiceByName/{serviceName}", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            DataObject = new
            {
                serviceName = "Parking Spot Updated",
                price = -5m,
                Description = "Negative price test"
            }
        });

        var responseText = await resp.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(resp, Has.Property("Status").EqualTo(400));
            Assert.That(responseText, Does.Contain("Price must be a positive value"));
        });
    }

    [Test]
    public async Task UpdateExtraServiceByName_Zeroprice_ReturnsBadRequest()
    {
        var serviceName = "Parking Spot";

        var resp = await Request.PutAsync($"/api/ExtraService/UpdateExtraServiceByName/{serviceName}", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            DataObject = new
            {
                serviceName = "Parking Spot Updated",
                price = 0m,
                Description = "Zero price test"
            }
        });

        var responseText = await resp.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(resp, Has.Property("Status").EqualTo(400));
            Assert.That(responseText, Does.Contain("Price must be a positive value"));
        });
    }

    [Test]
    public async Task UpdateExtraServiceByName_DuplicateName_ReturnsBadRequest()
    {
        var serviceName = "Parking Spot";
        var existingName = "Restaurant Access"; 

        var resp = await Request.PutAsync($"/api/ExtraService/UpdateExtraServiceByName/{serviceName}", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            DataObject = new
            {
                serviceName = existingName,
                price = 25m,
                Description = "Duplicate name test"
            }
        });

        var responseText = await resp.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(resp, Has.Property("Status").EqualTo(400));
            Assert.That(responseText, Does.Contain($"Extra service with the name {existingName} already exists"));
        });
    }


    [Test]
    public async Task DeleteExtraService_ExistingId_ReturnsOk()
    {
        int existingServiceId = 1;

        await using var response = await Request.DeleteAsync($"/api/ExtraService/DeleteExtraService/{existingServiceId}");
        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(200));
            Assert.That(responseText, Does.Contain($"Extra service with ID {existingServiceId} deleted successfully."));
        });
    }

    [Test]
    public async Task DeleteExtraService_NonExistingId_ReturnsNotFound()
    {
        int nonExistingServiceId = 9999;

        await using var response = await Request.DeleteAsync($"/api/ExtraService/DeleteExtraService/{nonExistingServiceId}");
        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(404));
            Assert.That(responseText, Does.Contain($"Extra service with ID {nonExistingServiceId} not found."));
        });
    }


    [Test]
    public async Task DeleteExtraService_ById_Twice_SecondTimeReturnsNotFound()
    {
        int serviceId = 2;

        var firstDelete = await Request.DeleteAsync($"/api/ExtraService/DeleteExtraService/{serviceId}");

        var secondDelete = await Request.DeleteAsync($"/api/ExtraService/DeleteExtraService/{serviceId}");
        var text = await secondDelete.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(firstDelete.Status, Is.EqualTo(200));
            Assert.That(secondDelete.Status, Is.EqualTo(404));
            Assert.That(text, Does.Contain($"Extra service with ID {serviceId} not found"));
        });
    }

    [TearDown]
    public async Task TearDownAPITesting()
    {
        await Request.DisposeAsync();
        await _context.DisposeAsync();
    }
}

