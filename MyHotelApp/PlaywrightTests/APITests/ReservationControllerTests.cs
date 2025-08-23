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
public class ReservationControllerTests : PlaywrightTest
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

        // Refresh the database
        await DatabaseRefresher.AddDataAsync(_context);
    }

    [Test]
    public async Task CreateReservation_ValidInput_ReturnsOkAndCreatesReservation()
    {
        await using var response = await Request.PostAsync("/api/Reservation/CreateReservation", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"}
            },
            DataObject = new
            {
                RoomNumber = 101,
                GuestID = "0123456789012",
                CheckInDate = DateTime.UtcNow,
                CheckOutDate = DateTime.UtcNow.AddDays(1),
                RoomServiceIDs = new[] { 1, 2 },
                ExtraServiceIDs = new[] { 3, 4 }
            }
        });

        if (response.Status != 200)
        {
            Assert.Fail($"Code: {response.Status} - {response.StatusText}");
        }

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(200));
            Assert.That(responseText, Does.Contain("Reservation created successfully for room 101."));
        });

    }

    [Test]
    public async Task CreateReservation_NonExistingRoom_ReturnsNotFound()
    {
        string nonExistingRoomNumber = "499"; //does not exist
        await using var response = await Request.PostAsync("/api/Reservation/CreateReservation", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"}
            },
            DataObject = new
            {
                RoomNumber = nonExistingRoomNumber,
                GuestID = "0123456789012",
                CheckInDate = DateTime.UtcNow,
                CheckOutDate = DateTime.UtcNow.AddDays(1)
            }
        });

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(404));
            Assert.That(responseText, Does.Contain($"Room with number {nonExistingRoomNumber} does not exist."));
        });
    }

    [Test]
    public async Task CreateReservation_RoomUnavailable_ReturnsBadRequest()
    {
        string unavailableRoomNumber = "101"; 
        DateTime unavailableCheckInDate = new DateTime(2025, 1, 13); //reserved from 10th to 14th
        DateTime unavailableCheckOutDate = new DateTime(2025, 1, 16);   
        await using var response = await Request.PostAsync("/api/Reservation/CreateReservation", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"}
            },
            DataObject = new
            {
                RoomNumber = unavailableRoomNumber,
                GuestID = "1234567890123", //valid
                CheckInDate = unavailableCheckInDate,
                CheckOutDate = unavailableCheckOutDate
            }
        });

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(400));
            Assert.That(responseText, Does.Contain("The room is already reserved for the selected dates."));
        });
    }

    [Test]
    public async Task GetReservations_NonEmptyReservations_ReturnsOk()
    {
        await using var response = await Request.GetAsync("/api/Reservation/GetAllReservations");

        if (response.Status != 200)
        {
            Assert.Fail($"Code: {response.Status} - {response.StatusText}");
        }
        //Assert.That(response.Status, Is.EqualTo(200), $"Expected 200 OK but got {response.Status} - {response.StatusText}");

        var jsonResult = await response.JsonAsync();

        // if (!jsonResult.HasValue || jsonResult.Value.ValueKind != System.Text.Json.JsonValueKind.Array)
        // {
        //     Assert.Fail("JSON response is not a valid array.");
        // }
        Assert.Multiple(() =>
        {
            Assert.That(jsonResult.HasValue, Is.True, "Response did not contain JSON.");
            Assert.That(jsonResult.Value.ValueKind, Is.EqualTo(JsonValueKind.Array), "Expected JSON array but got something else.");
        });

        var reservations = jsonResult.Value.EnumerateArray().ToList();

        Assert.That(reservations.Count, Is.GreaterThan(0), "Expected at least one reservation in the response.");
        //check just first why not 
        var firstReservation = reservations.First();
        Assert.Multiple(() =>
        {
            Assert.That(firstReservation.TryGetProperty("roomNumber", out var roomNumber) && roomNumber.GetInt32() != default);
            Assert.That(firstReservation.TryGetProperty("guestID", out var guestID) && !string.IsNullOrEmpty(guestID.GetString()));
            Assert.That(firstReservation.TryGetProperty("checkInDate", out var checkInDate) && checkInDate.GetDateTime() != default);
            Assert.That(firstReservation.TryGetProperty("checkOutDate", out var checkOutDate) && checkOutDate.GetDateTime() != default);
            Assert.That(firstReservation.TryGetProperty("totalPrice", out var totalPrice) && totalPrice.GetDecimal() != default);
        });
        // Console.WriteLine($"Room Number: {firstReservation.GetProperty("roomNumber").GetInt32()}");
        // Console.WriteLine($"Guest ID: {firstReservation.GetProperty("guestID").GetString()}");
        // Console.WriteLine($"Check-In Date: {firstReservation.GetProperty("checkInDate").GetDateTime()}");
        // Console.WriteLine($"Check-Out Date: {firstReservation.GetProperty("checkOutDate").GetDateTime()}");
        // Console.WriteLine($"Total Price: {firstReservation.GetProperty("totalPrice").GetDecimal()}");

    }

    [Test]
    public async Task GetReservation_ValidId_ReturnsOkAndReservation()
    {
        int validReservationId = 5;

        await using var response = await Request.GetAsync($"/api/Reservation/GetReservationById/{validReservationId}");

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

        var reservation = jsonResult.Value;
        //Console.WriteLine(reservation);

        Assert.Multiple(() =>
        {
            Assert.That(reservation.GetProperty("reservationID").GetInt32(), Is.EqualTo(5));
            Assert.That(reservation.GetProperty("roomNumber").GetInt32(), Is.EqualTo(301));
            Assert.That(reservation.GetProperty("guestID").GetString(), Is.EqualTo("5678901234567"));
            Assert.That(reservation.GetProperty("totalPrice").GetDecimal(), Is.EqualTo(257m));

            // Dates as strings then parsed
            var checkIn = DateTime.Parse(reservation.GetProperty("checkInDate").GetString()!);
            var checkOut = DateTime.Parse(reservation.GetProperty("checkOutDate").GetString()!);
            Assert.That(checkIn, Is.EqualTo(new DateTime(2025, 5, 12)));
            Assert.That(checkOut, Is.EqualTo(new DateTime(2025, 5, 14)));
        });

        var room = reservation.GetProperty("room");
        Assert.Multiple(() =>
        {
            Assert.That(room.GetProperty("roomNumber").GetInt32(), Is.EqualTo(301));
            Assert.That(room.GetProperty("floor").GetInt32(), Is.EqualTo(3));
            Assert.That(room.GetProperty("roomTypeID").GetInt32(), Is.EqualTo(3));
        });

        var guest = reservation.GetProperty("guest");
        Assert.Multiple(() =>
        {
            Assert.That(guest.GetProperty("jmbg").GetString(), Is.EqualTo("5678901234567"));
            Assert.That(guest.GetProperty("fullName").GetString(), Is.EqualTo("Jelena Kovačević"));
            Assert.That(guest.GetProperty("phoneNumber").GetString(), Is.EqualTo("+381658889999"));
        });

        var roomServices = reservation.GetProperty("roomServices").EnumerateArray().ToArray();
        Assert.That(roomServices.Length, Is.EqualTo(1));
        Assert.Multiple(() =>
        {
            var rs = roomServices[0];
            Assert.That(rs.GetProperty("roomServiceID").GetInt32(), Is.EqualTo(7));
            Assert.That(rs.GetProperty("itemName").GetString(), Is.EqualTo("Minibar"));
            Assert.That(rs.GetProperty("itemPrice").GetDecimal(), Is.EqualTo(5m));
            Assert.That(rs.GetProperty("description").GetString(), Is.EqualTo("Minibar items"));
        });

        var extraServices = reservation.GetProperty("extraServices").EnumerateArray().ToArray();
        Assert.That(extraServices.Length, Is.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(extraServices.Select(e => e.GetProperty("extraServiceID").GetInt32()), 
                Is.EquivalentTo(new[] { 2, 7 }));
            Assert.That(extraServices.Select(e => e.GetProperty("serviceName").GetString()), 
                Does.Contain("Restaurant Access").And.Contain("Breakfast Buffet"));
        });    
    }

    [Test]
    public async Task GetReservation_NonExistingId_ReturnsNotFound()
    {
        int invalidReservationId = 999;

        await using var response = await Request.GetAsync($"/api/Reservation/GetReservationById/{invalidReservationId}");
        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(404));
            Assert.That(responseText, Does.Contain($"Reservation with ID {invalidReservationId} not found."));
        });
    }
   

    [Test]
    public async Task UpdateReservation_ValidInput_ReturnsOkAndUpdatesReservation()
    {
        int existingReservationId = 1; //exists in db

        await using var response = await Request.PutAsync($"/api/Reservation/UpdateReservation/{existingReservationId}", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"}
            },
            DataObject = new
            {
                RoomNumber = 201, //bila 101
                GuestID = "1234567890123",
                CheckInDate = new DateTime(2025, 1, 10),
                CheckOutDate = new DateTime(2025, 1, 14),
                RoomServiceIDs = new[] { 1, 3, 5 },
                ExtraServiceIDs = new[] { 1, 2 }
            }
        });

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(200));
            Assert.That(responseText, Does.Contain($"Reservation with ID {existingReservationId} updated successfully."));
        });        
    }

    [Test]
    public async Task UpdateReservation_NonExistingId_ReturnsNotFound()
    {
        int invalidReservationId = 999;

        await using var response = await Request.PutAsync($"/api/Reservation/UpdateReservation/{invalidReservationId}", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"}
            },
            DataObject = new
            {
                RoomNumber = 201, //bila 101
                GuestID = "1234567890123",
                CheckInDate = new DateTime(2025, 1, 10),
                CheckOutDate = new DateTime(2025, 1, 14),
                RoomServiceIDs = new[] { 1, 3, 5 },
                ExtraServiceIDs = new[] { 1, 2 }
            }
        });

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(404));
            Assert.That(responseText, Does.Contain($"Reservation with ID {invalidReservationId} not found."));
        });
    }
    [Test]
    public async Task UpdateReservation_InvalidInput_ReturnsBadRequest()
    {
        int existingReservationId = 1; //exists in db
        string nonExistingGuest = "9999999999999"; //does not exist

        await using var response = await Request.PutAsync($"/api/Reservation/UpdateReservation/{existingReservationId}", new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"}
            },
            DataObject = new
            {
                RoomNumber = 101,
                GuestID = nonExistingGuest,
                CheckInDate = new DateTime(2025, 1, 10),
                CheckOutDate = new DateTime(2025, 1, 14),
                RoomServiceIDs = new[] { 1, 3, 5 },
                ExtraServiceIDs = new[] { 1, 2 }
            }
        });

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(404));
            Assert.That(responseText, Does.Contain($"Guest with ID {nonExistingGuest} does not exist."));
        });
    }
       
    [Test]
    public async Task DeleteReservation_ExistingId_ReturnsOk()
    {
        int existingReservationId = 1; //exists in db

        await using var response = await Request.DeleteAsync($"/api/Reservation/DeleteReservation/{existingReservationId}");

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(200));
            Assert.That(responseText, Does.Contain($"Reservation with ID {existingReservationId} deleted successfully."));
        });

        //check if its deleted
        await using var getResponse = await Request.GetAsync($"/api/Reservation/GetReservationById/{existingReservationId}");
        var getResponseText = await getResponse.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(getResponse, Has.Property("Status").EqualTo(404));
            Assert.That(getResponseText, Does.Contain($"Reservation with ID {existingReservationId} not found."));
        });
    }

    [Test]
    public async Task DeleteReservation_NonExistingId_ReturnsNotFound()
    {
        int nonExistingReservationId = 999; //does not exist in db

        await using var response = await Request.DeleteAsync($"/api/Reservation/DeleteReservation/{nonExistingReservationId}");

        var responseText = await response.TextAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response, Has.Property("Status").EqualTo(404));
            Assert.That(responseText, Does.Contain($"Reservation with ID {nonExistingReservationId} not found."));
        });
    }

    [TearDown]
    public async Task TearDownAPITesting()
    {
        await Request.DisposeAsync();
        await _context.DisposeAsync();
    }
}
