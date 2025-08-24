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
    public async Task CreateGuest_ValidInput_ReturnsOkAndCreatesGuest()
    {
        string validGuestJMBG = "0222456789012"; //new guest
        await using var response = await Request.PostAsync("/api/Guest/CreateGuest", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"}
            },
            DataObject = new
            {
                FullName = "Anita Aleksić",
                JMBG = validGuestJMBG,
                PhoneNumber = "+38161234567"
            }
        });

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(200));
            Assert.That(responseText, Does.Contain("Guest with JMBG 0222456789012 created successfully."));
        });

        // provera sto da ne 
        await using var getResponse = await Request.GetAsync($"/api/Guest/GetGuestByJMBG/{validGuestJMBG}");

        if (getResponse.Status != 200)
        {
            Assert.Fail($"Code: {getResponse.Status} - {getResponse.StatusText}");
        }

        var jsonResult = await getResponse.JsonAsync();

        Assert.Multiple(() =>
        {
            Assert.That(jsonResult.HasValue, Is.True, "Response did not contain JSON.");
            Assert.That(jsonResult.Value.ValueKind, Is.EqualTo(JsonValueKind.Object), "Expected JSON object but got something else.");
        });

        jsonResult.Value.TryGetProperty("fullName", out var fullName);
        jsonResult.Value.TryGetProperty("jmbg", out var jmbg);
        jsonResult.Value.TryGetProperty("phoneNumber", out var phoneNumber);

        Assert.Multiple(() =>
        {
            Assert.That(jmbg.GetString(), Is.EqualTo(validGuestJMBG));
            Assert.That(phoneNumber.GetString(), Is.EqualTo("+38161234567"));
            Assert.That(fullName.GetString(), Is.EqualTo("Anita Aleksić"));
        });        
    }

    [Test]
    public async Task CreateGuest_DuplicateJMBG_ReturnsBadRequest()
    {
        string existingJMBG = "0123456789012"; //exists in db 
        await using var response = await Request.PostAsync("/api/Guest/CreateGuest", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"}
            },
            DataObject = new
            {
                FullName = "Anita Aleksić",
                JMBG = existingJMBG,
                PhoneNumber = "+38161234567"
            }
        });

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(400));
            Assert.That(responseText, Does.Contain($"Guest with the same JMBG ({existingJMBG}) already exists."));
        });
    }

    [Test]
    public async Task CreateGuest_JMBGInvalidInput_ReturnsBadRequest()
    {
        await using var response = await Request.PostAsync("/api/Guest/CreateGuest", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"}
            },
            DataObject = new
            {
                FullName = "Anita Aleksić",
                JMBG = "invalid--jmbg",
                PhoneNumber = "+38161234567"
            }
        });

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(400));
            Assert.That(responseText, Does.Contain("JMBG must be exactly 13 characters long."));
        });
    }
               
    [Test]
    public async Task CreateGuest_FullNameInvalidInput_ReturnsBadRequest()
    {
        string name = new string('a', 101);
        await using var response = await Request.PostAsync("/api/Guest/CreateGuest", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"}
            },
            DataObject = new
            {
                FullName = name,
                JMBG = "1231231231231",
                PhoneNumber = "+38161234567"
            }
        });

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(400));
            Assert.That(responseText, Does.Contain("Full name is required and cannot exceed 100 characters."));
        });
    }

    [Test]
    public async Task CreateGuest_PhoneNumberInvalidInput_ReturnsBadRequest()
    {
        await using var response = await Request.PostAsync("/api/Guest/CreateGuest", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"}
            },
            DataObject = new
            {
                FullName = "Anita Aleksić",
                JMBG = "0222456789012",
                PhoneNumber = "+381612345aa"
            }
        });

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(400));
            Assert.That(responseText, Does.Contain("Invalid phone number."));
        });
    }

    [Test]
    public async Task CreateGuest_PhoneNumberTooLong_ReturnsBadRequest()
    {
        await using var response = await Request.PostAsync("/api/Guest/CreateGuest", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"}
            },
            DataObject = new
            {
                FullName = "Anita Aleksić",
                JMBG = "0222456789012",
                PhoneNumber = "+3816123454553"
            }
        });

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(400));
            Assert.That(responseText, Does.Contain("Phone number is required and must be between 12 and 13 characters long."));
        });
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

        Assert.Multiple(() =>
        {
            Assert.That(jsonResult.HasValue, Is.True, "Response did not contain JSON.");
            Assert.That(jsonResult.Value.ValueKind, Is.EqualTo(JsonValueKind.Array), "Expected JSON array but got something else.");
        });

        var guests = jsonResult.Value.EnumerateArray().ToList();

        Assert.That(guests.Count, Is.GreaterThan(0), "Expected at least one guest in the response.");

    }

    [Test]
    public async Task GetGuest_ValidJMBG_ReturnsOkAndGuest()
    {
        string validGuestJMBG = "0123456789012"; 

        await using var response = await Request.GetAsync($"/api/Guest/GetGuestByJMBG/{validGuestJMBG}");

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

        jsonResult.Value.TryGetProperty("fullName", out var fullName);
        jsonResult.Value.TryGetProperty("jmbg", out var jmbg);
        jsonResult.Value.TryGetProperty("phoneNumber", out var phoneNumber);

        Assert.Multiple(() =>
        {
            Assert.That(jmbg.GetString(), Is.EqualTo("0123456789012"));
            Assert.That(phoneNumber.GetString(), Is.EqualTo("+381696969696"));
            Assert.That(fullName.GetString(), Is.EqualTo("Lazar Živković"));
        });
    }

    [Test]
    public async Task GetGuest_NonExistingJMBG_ReturnsNotFound()
    {
        string invalidGuestJMBG = "9999999999999"; // Assuming this JMBG does not exist

        await using var response = await Request.GetAsync($"/api/Guest/GetGuestByJMBG/{invalidGuestJMBG}");
        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(404));
            Assert.That(responseText, Does.Contain($"Guest with JMBG {invalidGuestJMBG} not found."));
        });
    }

    [Test]
    public async Task GetGuest_TooLongJMBG_ReturnsBadRequest()
    {
        string invalidGuestJMBG = "99999999999999"; //14 characters

        await using var response = await Request.GetAsync($"/api/Guest/GetGuestByJMBG/{invalidGuestJMBG}");
        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(400));
            Assert.That(responseText, Does.Contain("JMBG must be exactly 13 characters long."));
        });
    }

    [Test]
    public async Task GetGuest_TooShortJMBG_ReturnsBadRequest()
    {
        string invalidGuestJMBG = "999999999999"; //12 characters

        await using var response = await Request.GetAsync($"/api/Guest/GetGuestByJMBG/{invalidGuestJMBG}");
        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(400));
            Assert.That(responseText, Does.Contain("JMBG must be exactly 13 characters long."));
        });
    }

    [Test]
    public async Task UpdateGuest_ValidInput_ReturnsOkAndUpdatesGuest()
    {
        string existingJMBG = "3456789012345"; //exists in db

        await using var response = await Request.PutAsync($"/api/Guest/UpdateGuest/{existingJMBG}", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"}
            },
            DataObject = new
            {
                FullName = "Ivana Aleksić", //bila je Ivana Nikolić
                JMBG = "0222456789012",
                PhoneNumber = "+381645556677"
            }
        });

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(200));
            Assert.That(responseText, Does.Contain($"Guest with JMBG {existingJMBG} updated successfully."));
        });

        //provera
        await using var getResponse = await Request.GetAsync($"/api/Guest/GetGuestByJMBG/{existingJMBG}");

        if (getResponse.Status != 200)
        {
            Assert.Fail($"Code: {getResponse.Status} - {getResponse.StatusText}");
        }

        var jsonResult = await getResponse.JsonAsync();

        Assert.Multiple(() =>
        {
            Assert.That(jsonResult.HasValue, Is.True, "Response did not contain JSON.");
            Assert.That(jsonResult.Value.ValueKind, Is.EqualTo(JsonValueKind.Object), "Expected JSON object but got something else.");
        });

        jsonResult.Value.TryGetProperty("fullName", out var fullName);
        jsonResult.Value.TryGetProperty("jmbg", out var jmbg);
        jsonResult.Value.TryGetProperty("phoneNumber", out var phoneNumber);

        Assert.Multiple(() =>
        {
            Assert.That(jmbg.GetString(), Is.EqualTo(existingJMBG));
            Assert.That(phoneNumber.GetString(), Is.EqualTo("+381645556677"));
            Assert.That(fullName.GetString(), Is.EqualTo("Ivana Aleksić"));
        });
    }

    [Test]
    public async Task UpdateGuest_NonExistingJMBG_ReturnsNotFound()
    {
        string invalidGuestJMBG = "9999999999999"; 

        await using var response = await Request.PutAsync($"/api/Guest/UpdateGuest/{invalidGuestJMBG}", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"}
            },
            DataObject = new
            {
                FullName = "Ivana Aleksić",
                JMBG = "9999999999999",
                PhoneNumber = "+381645556677"
            }
        });

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(404));
            Assert.That(responseText, Does.Contain($"Guest with JMBG {invalidGuestJMBG} not found."));
        });
    }

    [Test]
    public async Task UpdateGuest_FullNameTooLong_ReturnsBadRequest()
    {
        string existingJMBG = "3456789012345"; //exists in db
        string name = new string('a', 101);

        await using var response = await Request.PutAsync($"/api/Guest/UpdateGuest/{existingJMBG}", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"}
            },
            DataObject = new
            {
                FullName = name,
                JMBG = existingJMBG,
                PhoneNumber = "+381645556677"
            }
        });

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(400));
            Assert.That(responseText, Does.Contain("Full name is required and cannot exceed 100 characters."));
        });
    }
           
    [Test]
    public async Task UpdateGuest_PhoneNumberInvalidInput_ReturnsBadRequest()
    {
        string existingJMBG = "3456789012345"; //exists in db
        await using var response = await Request.PutAsync($"/api/Guest/UpdateGuest/{existingJMBG}", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"}
            },
            DataObject = new
            {
                FullName = "Anita Aleksić",
                JMBG = existingJMBG,
                PhoneNumber = "+381612345aa"
            }
        });

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(400));
            Assert.That(responseText, Does.Contain("Invalid phone number."));
        });
    }

    [Test]
    public async Task DeleteGuest_ExistingJMBG_ReturnsOk()
    {
        string existingJMBG = "1234567890123"; //exists in db

        await using var response = await Request.DeleteAsync($"/api/Guest/DeleteGuest/{existingJMBG}");

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(200));
            Assert.That(responseText, Does.Contain($"Guest with JMBG {existingJMBG} deleted successfully."));
        });

        await using var getResponse = await Request.GetAsync($"/api/Guest/GetGuestByJMBG/{existingJMBG}");
        var getResponseText = await getResponse.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(getResponse, Has.Property("Status").EqualTo(404));
            Assert.That(getResponseText, Does.Contain($"Guest with JMBG {existingJMBG} not found."));
        });
    }

    [Test]
    public async Task DeleteGuest_NonExistingJMBG_ReturnsNotFound()
    {
        string nonExistingJMBG = "9999999999999"; //does not exist in db

        await using var response = await Request.DeleteAsync($"/api/Guest/DeleteGuest/{nonExistingJMBG}");

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(404));
            Assert.That(responseText, Does.Contain($"Guest with JMBG {nonExistingJMBG} not found."));
        });
    }

    [TearDown]
    public async Task TearDownAPITesting()
    {
        await Request.DisposeAsync();
        await _context.DisposeAsync();
    }
}
