// using Microsoft.Playwright.NUnit;
// using Microsoft.Playwright;

// using Microsoft.EntityFrameworkCore.Storage;
// using Microsoft.EntityFrameworkCore.Diagnostics;
// using PlaywrightTests;
// using NUnit.Framework;

// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using MyHotelApp.server.Models;
// using MyHotelApp.Controllers;

// using System.Threading.Tasks;

// using System.Text.Json;
// using System.Text.Json.Nodes;
// using System.Text;


// namespace PlaywrightTests.APITests;


// [TestFixture]
// public class ExtraServiceControllerTests : PlaywrightTest
// {
//     private IAPIRequestContext Request;
//     private HotelContext _context;
//     //var page = await context.NewPageAsync();


//     [SetUp]
//     public async Task SetUpAPITesting()
//     {
//         var headers = new Dictionary<string, string>
//         {
//             {"Accept", "application/json"},
//         };

//         Request = await Playwright.APIRequest.NewContextAsync(new()
//         {
//             BaseURL = "http://localhost:5023",
//             ExtraHTTPHeaders = headers,
//             IgnoreHTTPSErrors = true
//         });

//         var optionsBuilder = new DbContextOptionsBuilder<HotelContext>();
//         optionsBuilder.UseSqlServer("Server=(localdb)\\HotelDatabase;Database=Hotel");

//         var options = optionsBuilder.Options;
//         _context = new HotelContext(options);

//         // Refresh the database
//         await DatabaseRefresher.AddDataAsync(_context);
//     }

//     [Test]
//     public async Task GetExtraServices_DisplaysApiExtraServicesInTable()
//     {
//         await using var response = await Request.GetAsync("/api/ExtraService/GetAllExtraServices",
//             new APIRequestContextOptions { Headers = new Dictionary<string, string> { { "Accept", "application/json" } } });

//         if (response.Status != 200)
//         {
//             Assert.Fail($"Code: {response.Status} - {response.StatusText}");
//         }

//         var jsonResult = await response.JsonAsync();

//         if (!jsonResult.HasValue || jsonResult.Value.ValueKind != System.Text.Json.JsonValueKind.Array)
//         {
//             Assert.Fail("JSON response is not a valid array.");
//         }

//         var extraServices = jsonResult.Value.EnumerateArray().ToList();

//         Assert.That(extraServices.Count, Is.GreaterThan(0), "Expected at least one extra service in the response.");

//         var firstService = extraServices.First();

//         if (!(firstService.TryGetProperty("extraServiceID", out var extraServiceID) &&
//             firstService.TryGetProperty("serviceName", out var serviceName) &&
//             firstService.TryGetProperty("price", out var price) &&
//             firstService.TryGetProperty("description", out var description)))
//         {
//             Assert.Fail("JSON does not contain expected ExtraService properties.");
//         }
//         else
//         {
//             Assert.Multiple(() =>
//             {
//                 Assert.That(extraServiceID.GetInt32(), Is.GreaterThan(0), "extraServiceID should be greater than 0.");
//                 Assert.That(!string.IsNullOrEmpty(serviceName.GetString()), "serviceName should not be null or empty.");
//                 Console.WriteLine($"Service Name: {serviceName.GetString()}");
//                 Assert.That(price.GetDecimal(), Is.GreaterThan(0), "price should be greater than 0.");
//                 Assert.That(description.ValueKind == System.Text.Json.JsonValueKind.String
//                             || description.ValueKind == System.Text.Json.JsonValueKind.Null,
//                             "description should be a string or null.");
//             });
//         }
//     }

//     [Test]
//     public async Task GetExtraServiceById_ExistingId_ReturnsOkWithCorrectData()
//     {
//         int serviceId = 1;
//         string expectedName = "Parking"; 

//         var resp = await Request.GetAsync($"/api/ExtraService/GetExtraServiceById/{serviceId}",
//             new APIRequestContextOptions { Headers = new Dictionary<string, string> { { "Accept", "application/json" } } });

//         var text = await resp.TextAsync();

//         Console.WriteLine($"GET ExtraService by ID {serviceId} => Status: {resp.Status}");
//         Console.WriteLine($"Response: {text}");

//         Assert.Multiple(() =>
//         {
//             Assert.That(resp.Status, Is.EqualTo(200));
//             Assert.That(text, Does.Contain(expectedName));
//         });
//     }

//     [Test]
//     public async Task GetExtraServiceById_NonExistingId_ReturnsNotFound()
//     {
//         var nonExistingId = 9999;

//         var resp = await Request.GetAsync($"/api/ExtraService/GetExtraServiceById/{nonExistingId}",
//             new APIRequestContextOptions { Headers = new Dictionary<string, string> { { "Accept", "application/json" } } });

//         var text = await resp.TextAsync();

//         Console.WriteLine($"GET non-existing ExtraService by ID {nonExistingId} => Status: {resp.Status}");
//         Console.WriteLine($"Response: {text}");

//         Assert.Multiple(() =>
//         {
//             Assert.That(resp.Status, Is.EqualTo(404));
//             Assert.That(text, Does.Contain($"Extra service with ID {nonExistingId} not found"));
//         });
//     }

//     [Test]
//     public async Task GetExtraServiceByName_ExistingName_ReturnsOkWithCorrectData()
//     {
//         string serviceName = "Parking Spot";

//         var resp = await Request.GetAsync($"/api/ExtraService/GetExtraServiceByName/{serviceName}",
//             new APIRequestContextOptions { Headers = new Dictionary<string, string> { { "Accept", "application/json" } } });

//         var text = await resp.TextAsync();

//         Console.WriteLine($"GET ExtraService by Name '{serviceName}' => Status: {resp.Status}");
//         Console.WriteLine($"Response: {text}");

//         Assert.Multiple(() =>
//         {
//             Assert.That(resp.Status, Is.EqualTo(200));
//             Assert.That(text, Does.Contain(serviceName));
//         });
//     }

//     [Test]
//     public async Task GetExtraServiceByName_NonExistingName_ReturnsNotFound()
//     {
//         var nonExistingName = "ThisServiceDoesNotExist";

//         var resp = await Request.GetAsync($"/api/ExtraService/GetExtraServiceByName/{nonExistingName}",
//             new APIRequestContextOptions { Headers = new Dictionary<string, string> { { "Accept", "application/json" } } });

//         var text = await resp.TextAsync();

//         Console.WriteLine($"GET non-existing ExtraService by Name '{nonExistingName}' => Status: {resp.Status}");
//         Console.WriteLine($"Response: {text}");

//         Assert.Multiple(() =>
//         {
//             Assert.That(resp.Status, Is.EqualTo(404));
//             Assert.That(text, Does.Contain($"Extra service with name {nonExistingName} not found"));
//         });
//     }

//     [Test]
//     public async Task CreateExtraService_ValidData_CheckDatabase()
//     {
//         var serviceName = "TestExtraService";

        
//         var createResponse = await Request.PostAsync("/api/ExtraService/CreateExtraService", new APIRequestContextOptions
//         {
//             Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
//             DataObject = new
//             {
//                 ServiceName = serviceName,
//                 Price = 150m,
//                 Description = "Test extra service description"
//             }
//         });

//         Assert.That(createResponse.Status, Is.EqualTo(200));
//         var createText = await createResponse.TextAsync();
//         Assert.That(createText, Is.EqualTo($"\"Extra service with name {serviceName} created successfully.\""));

        
//         var getResponse = await Request.GetAsync($"/api/ExtraService/GetExtraServiceByName/{serviceName}");
//         Assert.That(getResponse.Status, Is.EqualTo(200));

//         var jsonResult = await getResponse.JsonAsync();
//         Assert.That(jsonResult.HasValue, Is.True);

//         Assert.That(jsonResult.Value.TryGetProperty("serviceName", out var returnedName) && returnedName.GetString() == serviceName, Is.True);
//         Assert.That(jsonResult.Value.TryGetProperty("price", out var returnedPrice) && returnedPrice.GetDecimal() == 150m, Is.True);
//         Assert.That(jsonResult.Value.TryGetProperty("description", out var returnedDescription) && returnedDescription.GetString() == "Test extra service description", Is.True);
//     }

//     [Test]
//     public async Task CreateExtraService_EmptyName_ReturnsBadRequest()
//     {
//         var resp = await Request.PostAsync("/api/ExtraService/CreateExtraService", new APIRequestContextOptions
//         {
//             Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
//             DataObject = new
//             {
//                 ServiceName = "",
//                 Price = 10m,
//                 Description = "Empty name test"
//             }
//         });

//         var body = await resp.BodyAsync();
//         var jsonString = Encoding.UTF8.GetString(body);

//         Console.WriteLine($"POST EmptyName => Status: {resp.Status}");
//         Console.WriteLine($"Response: {jsonString}");

//         Assert.Multiple(() =>
//         {
//             Assert.That(resp, Has.Property("Status").EqualTo(400));
//             Assert.That(jsonString, Does.Contain("Service name is required and cannot exceed 50 characters"));
//         });
//     }

//     [Test]
//     public async Task CreateExtraService_NameTooLong_ReturnsBadRequest()
//     {
//         var resp = await Request.PostAsync("/api/ExtraService/CreateExtraService", new APIRequestContextOptions
//         {
//             Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
//             DataObject = new
//             {
//                 ServiceName = new string('a', 51),
//                 Price = 10m,
//                 Description = "Name too long test"
//             }
//         });

//         var body = await resp.BodyAsync();
//         var jsonString = Encoding.UTF8.GetString(body);

//         Console.WriteLine($"POST NameTooLong => Status: {resp.Status}");
//         Console.WriteLine($"Response: {jsonString}");

//         Assert.Multiple(() =>
//         {
//             Assert.That(resp, Has.Property("Status").EqualTo(400));
//             Assert.That(jsonString, Does.Contain("Service name is required and cannot exceed 50 characters"));
//         });
//     }

//     [Test]
//     public async Task CreateExtraService_NegativePrice_ReturnsBadRequest()
//     {
//         var resp = await Request.PostAsync("/api/ExtraService/CreateExtraService", new APIRequestContextOptions
//         {
//             Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
//             DataObject = new
//             {
//                 ServiceName = "NegativePriceExtra",
//                 Price = -5m,
//                 Description = "Negative price test"
//             }
//         });

//         var body = await resp.BodyAsync();
//         var jsonString = Encoding.UTF8.GetString(body);

//         Console.WriteLine($"POST NegativePrice => Status: {resp.Status}");
//         Console.WriteLine($"Response: {jsonString}");

//         Assert.Multiple(() =>
//         {
//             Assert.That(resp, Has.Property("Status").EqualTo(400));
//             Assert.That(jsonString, Does.Contain("Price must be a positive value"));
//         });
//     }

//     [Test]
//     public async Task CreateExtraService_ZeroPrice_ReturnsBadRequest()
//     {
//         var resp = await Request.PostAsync("/api/ExtraService/CreateExtraService", new APIRequestContextOptions
//         {
//             Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
//             DataObject = new
//             {
//                 ServiceName = "ZeroPriceExtra",
//                 Price = 0m,
//                 Description = "Zero price test"
//             }
//         });

//         var body = await resp.BodyAsync();
//         var jsonString = Encoding.UTF8.GetString(body);

//         Console.WriteLine($"POST ZeroPrice => Status: {resp.Status}");
//         Console.WriteLine($"Response: {jsonString}");

//         Assert.Multiple(() =>
//         {
//             Assert.That(resp, Has.Property("Status").EqualTo(400));
//             Assert.That(jsonString, Does.Contain("Price must be a positive value"));
//         });
//     }

//     [Test]
//     public async Task CreateExtraService_DuplicateName_ReturnsError()
//     {
//         var existingName = "Parking Spot"; // već postoji u bazi
//         var createResp = await Request.PostAsync("/api/ExtraService/CreateExtraService", new APIRequestContextOptions
//         {
//             Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
//             DataObject = new { ServiceName = existingName, Price = 100.0m, Description = "Duplicate Test" }
//         });

//         Console.WriteLine($"Create duplicate response status: {createResp.Status}");
//         var text = await createResp.TextAsync();
//         Console.WriteLine($"Response text: {text}");

//         Assert.That(createResp.Status, Is.EqualTo(400));
//         Assert.That(text, Does.Contain($"Extra service with the name {existingName} already exists."));
//     }


//     [Test]
//     public async Task UpdateExtraService_ById_UpdatesSuccessfully()
//     {
//         var service = await _context.ExtraServices.FirstAsync(es => es.ServiceName == "Parking Spot");
//         var updatedName = "Parking Premium";

//         var updateResp = await Request.PutAsync($"/api/ExtraService/UpdateExtraService/{service.ExtraServiceID}", new APIRequestContextOptions
//         {
//             Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
//             DataObject = new
//             {
//                 ServiceName = updatedName,
//                 Price = 50.00m,
//                 Description = "Updated description"
//             }
//         });

//         Assert.That(updateResp.Status, Is.EqualTo(200));
//         Console.WriteLine($"Update response status: {updateResp.Status}");

//         var getUpdated = await Request.GetAsync($"/api/ExtraService/GetExtraServiceById/{service.ExtraServiceID}");
//         var json = await getUpdated.JsonAsync();
//         Assert.That(json.HasValue, Is.True);

//         Console.WriteLine($"Updated JSON: {json.Value}");

//         Assert.Multiple(() =>
//         {
//             Assert.That(json.Value.TryGetProperty("extraServiceID", out var idProp) && idProp.GetInt32() == service.ExtraServiceID, "extraServiceID mismatch");
//             Assert.That(json.Value.TryGetProperty("serviceName", out var nameProp) && nameProp.GetString() == updatedName, "serviceName mismatch");
//             Assert.That(json.Value.TryGetProperty("price", out var priceProp) && priceProp.GetDecimal() == 50.00m, "price mismatch");
//             Assert.That(json.Value.TryGetProperty("description", out var descProp) && descProp.GetString() == "Updated description", "description mismatch");
//         });
//     }

//     [Test]
//     public async Task UpdateExtraServiceByName_ValidUpdate_Succeeds()
//     {
//         var serviceName = "Parking Spot";
//         var updatedName = "Parking Premium";

//         var resp = await Request.PutAsync($"/api/ExtraService/UpdateExtraServiceByName/{serviceName}", new APIRequestContextOptions
//         {
//             Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
//             DataObject = new
//             {
//                 ServiceName = updatedName,
//                 Price = 60m,
//                 Description = "Updated description"
//             }
//         });

//         var body = await resp.BodyAsync();
//         var jsonString = Encoding.UTF8.GetString(body);

//         Console.WriteLine($"PUT ValidUpdate => Status: {resp.Status}");
//         Console.WriteLine($"Response: {jsonString}");

//         Assert.Multiple(() =>
//         {
//             Assert.That(resp, Has.Property("Status").EqualTo(200));
//             Assert.That(jsonString, Does.Contain($"Extra service with name {serviceName} updated successfully"));
//         });
//     }

//     [Test]
//     public async Task UpdateExtraServiceByName_EmptyName_ReturnsBadRequest()
//     {
//         var serviceName = "Parking Spot";

//         var resp = await Request.PutAsync($"/api/ExtraService/UpdateExtraServiceByName/{serviceName}", new APIRequestContextOptions
//         {
//             Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
//             DataObject = new
//             {
//                 ServiceName = "",
//                 Price = 60m,
//                 Description = "Empty name test"
//             }
//         });

//         var body = await resp.BodyAsync();
//         var jsonString = Encoding.UTF8.GetString(body);

//         Console.WriteLine($"PUT EmptyName => Status: {resp.Status}");
//         Console.WriteLine($"Response: {jsonString}");

//         Assert.Multiple(() =>
//         {
//             Assert.That(resp, Has.Property("Status").EqualTo(400));
//             Assert.That(jsonString, Does.Contain("Service name is required and cannot exceed 50 characters"));
//         });
//     }

//     [Test]
//     public async Task UpdateExtraServiceByName_NameTooLong_ReturnsBadRequest()
//     {
//         var serviceName = "Parking Spot";

//         var resp = await Request.PutAsync($"/api/ExtraService/UpdateExtraServiceByName/{serviceName}", new APIRequestContextOptions
//         {
//             Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
//             DataObject = new
//             {
//                 ServiceName = new string('a', 51),
//                 Price = 60m,
//                 Description = "Name too long test"
//             }
//         });

//         var body = await resp.BodyAsync();
//         var jsonString = Encoding.UTF8.GetString(body);

//         Console.WriteLine($"PUT NameTooLong => Status: {resp.Status}");
//         Console.WriteLine($"Response: {jsonString}");

//         Assert.Multiple(() =>
//         {
//             Assert.That(resp, Has.Property("Status").EqualTo(400));
//             Assert.That(jsonString, Does.Contain("Service name is required and cannot exceed 50 characters"));
//         });
//     }

//     [Test]
//     public async Task UpdateExtraServiceByName_NegativePrice_ReturnsBadRequest()
//     {
//         var serviceName = "Parking Spot";

//         var resp = await Request.PutAsync($"/api/ExtraService/UpdateExtraServiceByName/{serviceName}", new APIRequestContextOptions
//         {
//             Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
//             DataObject = new
//             {
//                 ServiceName = "Parking Updated",
//                 Price = -5m,
//                 Description = "Negative price test"
//             }
//         });

//         var body = await resp.BodyAsync();
//         var jsonString = Encoding.UTF8.GetString(body);

//         Console.WriteLine($"PUT NegativePrice => Status: {resp.Status}");
//         Console.WriteLine($"Response: {jsonString}");

//         Assert.Multiple(() =>
//         {
//             Assert.That(resp, Has.Property("Status").EqualTo(400));
//             Assert.That(jsonString, Does.Contain("Price must be a positive value"));
//         });
//     }

//     [Test]
//     public async Task UpdateExtraServiceByName_ZeroPrice_ReturnsBadRequest()
//     {
//         var serviceName = "Parking Spot";

//         var resp = await Request.PutAsync($"/api/ExtraService/UpdateExtraServiceByName/{serviceName}", new APIRequestContextOptions
//         {
//             Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
//             DataObject = new
//             {
//                 ServiceName = "Parking Updated",
//                 Price = 0m,
//                 Description = "Zero price test"
//             }
//         });

//         var body = await resp.BodyAsync();
//         var jsonString = Encoding.UTF8.GetString(body);

//         Console.WriteLine($"PUT ZeroPrice => Status: {resp.Status}");
//         Console.WriteLine($"Response: {jsonString}");

//         Assert.Multiple(() =>
//         {
//             Assert.That(resp, Has.Property("Status").EqualTo(400));
//             Assert.That(jsonString, Does.Contain("Price must be a positive value"));
//         });
//     }

//     [Test]
//     public async Task UpdateExtraServiceByName_DuplicateName_ReturnsBadRequest()
//     {
//         var serviceName = "Parking Spot";
//         var existingName = "Restaurant Access"; // već postoji u bazi

//         var resp = await Request.PutAsync($"/api/ExtraService/UpdateExtraServiceByName/{serviceName}", new APIRequestContextOptions
//         {
//             Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
//             DataObject = new
//             {
//                 ServiceName = existingName,
//                 Price = 70m,
//                 Description = "Duplicate name test"
//             }
//         });

//         var body = await resp.BodyAsync();
//         var jsonString = Encoding.UTF8.GetString(body);

//         Console.WriteLine($"PUT DuplicateName => Status: {resp.Status}");
//         Console.WriteLine($"Response: {jsonString}");

//         Assert.Multiple(() =>
//         {
//             Assert.That(resp, Has.Property("Status").EqualTo(400));
//             Assert.That(jsonString, Does.Contain($"Extra service with the name {existingName} already exists."));
//         });
//     }

//     [Test]
//     public async Task DeleteExtraService_ById_DeletesSuccessfully()
//     {
//         int serviceId = 1;

//         var deleteResp = await Request.DeleteAsync($"/api/ExtraService/DeleteExtraService/{serviceId}");
//         Console.WriteLine($"Delete response status: {deleteResp.Status}");

//         var getDeleted = await Request.GetAsync($"/api/ExtraService/GetExtraServiceById/{serviceId}");
//         Console.WriteLine($"GET after delete status: {getDeleted.Status}");

//         Assert.Multiple(() =>
//         {
//             Assert.That(deleteResp.Status, Is.EqualTo(200));
//             Assert.That(getDeleted.Status, Is.EqualTo(404));
//         });
//     }

//     [Test]
//     public async Task DeleteExtraService_ById_NonExistingId_ReturnsNotFound()
//     {
//         int nonExistingId = 9999;

//         var deleteResp = await Request.DeleteAsync($"/api/ExtraService/DeleteExtraService/{nonExistingId}");
//         var text = await deleteResp.TextAsync();
//         Console.WriteLine($"Delete non-existing ID response status: {deleteResp.Status}");
//         Console.WriteLine($"Response text: {text}");

//         Assert.Multiple(() =>
//         {
//             Assert.That(deleteResp.Status, Is.EqualTo(404));
//             Assert.That(text, Does.Contain($"Extra service with ID {nonExistingId} not found"));
//         });
//     }

//     [Test]
//     public async Task DeleteExtraService_ById_Twice_SecondTimeReturnsNotFound()
//     {
//         int serviceId = 2;

//         // Prvi DELETE
//         var firstDelete = await Request.DeleteAsync($"/api/ExtraService/DeleteExtraService/{serviceId}");
//         Console.WriteLine($"First delete response status: {firstDelete.Status}");

//         // Drugi DELETE
//         var secondDelete = await Request.DeleteAsync($"/api/ExtraService/DeleteExtraService/{serviceId}");
//         var text = await secondDelete.TextAsync();
//         Console.WriteLine($"Second delete response status: {secondDelete.Status}");
//         Console.WriteLine($"Response text: {text}");

//         Assert.Multiple(() =>
//         {
//             Assert.That(firstDelete.Status, Is.EqualTo(200));
//             Assert.That(secondDelete.Status, Is.EqualTo(404));
//             Assert.That(text, Does.Contain($"Extra service with ID {serviceId} not found"));
//         });
//     }


//     [TearDown]
//     public async Task TearDownAPITesting()
//     {
//         await Request.DisposeAsync();
//         await _context.DisposeAsync();
//     }
// }