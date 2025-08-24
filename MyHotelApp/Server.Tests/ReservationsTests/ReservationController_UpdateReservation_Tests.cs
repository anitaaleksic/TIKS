using Microsoft.EntityFrameworkCore;
using MyHotelApp.server.Models;
using MyHotelApp.Controllers;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Collections.Generic;
using Microsoft.Identity.Client;
using System.Threading.Tasks;

namespace ReservationTests;

[TestFixture]
public class ReservationController_UpdateReservation_Tests
{
    private static HotelContext _context;
    private static ReservationController _controllerReservation;
    private static RoomServiceController _controllerRoomService;
    private static ExtraServiceController _controllerExtraService;
    private static RoomController _controllerRoom;
    private static GuestController _controllerGuest;
    //private Room _room;
    private RoomService _roomService;
    private ExtraService _extraService;

    [SetUp]
    public async Task SetUp()
    {
        var options = new DbContextOptionsBuilder<HotelContext>()
                    .UseInMemoryDatabase(databaseName: "HotelTestDb")
                    .Options;
        _context = new HotelContext(options);
        _controllerReservation = new ReservationController(_context);
        _controllerRoomService = new RoomServiceController(_context);
        _controllerExtraService = new ExtraServiceController(_context);
        _controllerGuest = new GuestController(_context);
        _controllerRoom = new RoomController(_context);

        //seed db
        _context.Guests.Add(new Guest
        {
            JMBG = "1234512345123",
            FullName = "Anita Aleksic",
            PhoneNumber = "+381651234567"
        });
        _context.SaveChanges();

        _context.Guests.Add(new Guest
        {
            JMBG = "1234567890123",
            FullName = "Mila Aleksic",
            PhoneNumber = "+381651234567"
        });
        _context.SaveChanges();

        _context.RoomTypes.Add(new RoomType
        {
            RoomTypeID = 1,
            Type = "single",
            Capacity = 1,
            PricePerNight = 120.00m
        });
        _context.SaveChanges();

        Room room = new Room
        {
            RoomNumber = 123,
            RoomTypeID = 1,
            Floor = 1
        };

        _context.Rooms.Add(room);
        _context.SaveChanges();

        Room room1 = new Room
        {
            RoomNumber = 699,
            RoomTypeID = 1,
            Floor = 6
        };

        _context.Rooms.Add(room1);
        _context.SaveChanges();

        Room room2 = new Room
        {
            RoomNumber = 500,
            RoomTypeID = 1,
            Floor = 5
        };

        _context.Rooms.Add(room2);
        _context.SaveChanges();

        _roomService = new RoomService
        {
            RoomServiceID = 1,
            ItemName = "Breakfast",
            ItemPrice = 10m,
            Description = "Continental breakfast"
        };
        _context.RoomServices.Add(_roomService);

        _context.SaveChanges();

        _context.RoomServices.Add(new RoomService
        {
            RoomServiceID = 2,
            ItemName = "Laundry",
            ItemPrice = 15m,
            Description = "Laundry service"
        });

        _context.SaveChanges();

        _extraService = new ExtraService
        {
            ExtraServiceID = 1,
            ServiceName = "Parking Spot",
            Price = 20m,
            Description = "Reserved parking space"
        };

        _context.ExtraServices.Add(_extraService);

        _context.SaveChanges();

        ExtraService extraService2 = new ExtraService
        {
            ExtraServiceID = 2,
            ServiceName = "Restaurant Access",
            Price = 25m,
            Description = "Access to hotel restaurant"
        };

        _context.ExtraServices.Add(extraService2);

        _context.SaveChanges();

        ReservationDTO reservation = new ReservationDTO
        {
            ReservationID = 1,
            RoomNumber = 699,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3),
            RoomServiceIDs = { 1 }
        };
        await _controllerReservation.CreateReservation(reservation);

        ReservationDTO reservation2 = new ReservationDTO
        {
            ReservationID = 2,
            RoomNumber = 123,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3),
            ExtraServiceIDs = { 1 }
        };
        await _controllerReservation.CreateReservation(reservation2);
        //za overlaping dates 
        ReservationDTO reservation3 = new ReservationDTO
        {
            ReservationID = 3,
            RoomNumber = 699,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 5),
            CheckOutDate = new DateTime(2025, 9, 7),
            RoomServiceIDs = { 1 }
        };
        await _controllerReservation.CreateReservation(reservation3);


    }

    [Test] //add to all Create tests
    public async Task UpdateReservation_WithModelStateInvalid_ReturnsBadRequest()
    {
        // Arrange
        _controllerReservation.ModelState.AddModelError("error", "some model state error");
        var reservationDTO = new ReservationDTO();

        var result = await _controllerReservation.UpdateReservation(1, reservationDTO);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateReservation_WithRoomNumberTooSmall_ReturnsBadRequest()
    {
        var smallRoomNum = 100;
        ReservationDTO reservationDTO = new ReservationDTO
        {
            RoomNumber = smallRoomNum,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3)
        };

        var result = await _controllerReservation.UpdateReservation(1, reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badReqObj = result as BadRequestObjectResult;
            Assert.That(badReqObj, Has.Property("Value").EqualTo("Room number must be between 101 and 699."));
        });        
    }

    [Test]
    public async Task UpdateReservation_WithRoomNumberTooLarge_ReturnsBadRequest()
    {
        var largeRoomNum = 700;
        ReservationDTO reservationDTO = new ReservationDTO
        {
            RoomNumber = largeRoomNum,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3)
        };

        var result = await _controllerReservation.UpdateReservation(1, reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badReqObj = result as BadRequestObjectResult;
            Assert.That(badReqObj, Has.Property("Value").EqualTo("Room number must be between 101 and 699."));
        });        
    }

    [Test]
    public async Task UpdateReservation_WithRoomNumberOnLimit_CreatesReservation()
    {
        var limitRoomNum = 699;
        ReservationDTO reservationDTO = new ReservationDTO
        {
            RoomNumber = limitRoomNum,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 10, 1),
            CheckOutDate = new DateTime(2025, 10, 3)
        };

        var result = await _controllerReservation.UpdateReservation(1, reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Has.Property("Value").EqualTo($"Reservation with ID 1 updated successfully."));
        });        
    }

    [Test]
    public async Task UpdateReservation_WithNonExistingRoom_ReturnsNotFound()
    {
        var nonExistingRoomNum = 200;
        ReservationDTO reservationDTO = new ReservationDTO
        {
            RoomNumber = nonExistingRoomNum,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3)
        };

        var result = await _controllerReservation.UpdateReservation(1, reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
            var notFoundObj = result as NotFoundObjectResult;
            Assert.That(notFoundObj, Has.Property("Value").EqualTo($"Room with number {nonExistingRoomNum} does not exist."));
        });        
    }

    [Test]
    public async Task UpdateReservation_WithNonExistingGuest_ReturnsNotFound()
    {
        var nonExistingGuest = "9999999999999";
        ReservationDTO reservationDTO = new ReservationDTO
        {
            RoomNumber = 123,
            GuestID = nonExistingGuest,
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3),
            TotalPrice = 120.00m
        };

        var result = await _controllerReservation.UpdateReservation(1, reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
            var notFoundObj = result as NotFoundObjectResult;
            Assert.That(notFoundObj, Has.Property("Value").EqualTo($"Guest with ID {nonExistingGuest} does not exist."));
        });        
    }

    [Test]
    public async Task UpdateReservation_WithGuestIDInvalid_ReturnsBadRequest()
    {
        var invalidGuestId = "999999999999";//12 characters
        ReservationDTO reservationDTO = new ReservationDTO
        {
            RoomNumber = 123,
            GuestID = invalidGuestId,
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3),
            TotalPrice = 120.00m
        };

        var result = await _controllerReservation.UpdateReservation(1, reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badReqObj = result as BadRequestObjectResult;
            Assert.That(badReqObj, Has.Property("Value").EqualTo("JMBG must be exactly 13 characters long."));
        });          
    }

    [Test]
    public async Task UpdateReservation_WithCheckInOutDatesInvalid_ReturnsBadRequest()
    {
        ReservationDTO reservationDTO = new ReservationDTO
        {
            RoomNumber = 123,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 2),
            CheckOutDate = new DateTime(2025, 9, 1)
        };

        var result = await _controllerReservation.UpdateReservation(1, reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badReqObj = result as BadRequestObjectResult;
            Assert.That(badReqObj, Has.Property("Value").EqualTo("Check-out date must be after check-in date."));
        });        
    }

    [Test]
    public async Task UpdateReservation_WithCheckInOutDatesEqual_ReturnsBadRequest()
    {
        ReservationDTO reservationDTO = new ReservationDTO
        {
            RoomNumber = 123,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 1),
            TotalPrice = 120.00m
        };

        var result = await _controllerReservation.UpdateReservation(1, reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badReqObj = result as BadRequestObjectResult;
            Assert.That(badReqObj, Has.Property("Value").EqualTo("Check-out date must be after check-in date."));
        });        
    }

    [Test]
    public async Task UpdateReservation_WithOverlapingReservationDates_ReturnsBadRequest()
    {
         ReservationDTO reservationDTO = new ReservationDTO
        {
            RoomNumber = 699,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 6),
            CheckOutDate = new DateTime(2025, 9, 8)
        };
        //imam jednu rez za tu sobu od 5. do 7.

        var result = await _controllerReservation.UpdateReservation(1, reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badReqObj = result as BadRequestObjectResult;
            Assert.That(badReqObj, Has.Property("Value").EqualTo("The room is already reserved for the selected dates."));
        });        
    }

    [Test]
    public async Task UpdateReservation_WithAlmostOverlapingReservationDates_CreatesReservation()
    {
        ReservationDTO reservationDTO = new ReservationDTO
        {
            RoomNumber = 699,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 7),
            CheckOutDate = new DateTime(2025, 9, 9)
        };
        //imam jednu rez za tu sobu od 5. do 7.

        var result = await _controllerReservation.UpdateReservation(1, reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Has.Property("Value").EqualTo($"Reservation with ID 1 updated successfully."));
        });       
    }

    //ovde je provereno i da ce es da obrise ako ga ne posaljemo u update

    //provera je l izbacilo rez iz es ???????????
    [Test]//da li je potrebna jedna provera kad je lista prazna jedna kad vec ima neki rs
    public async Task UpdateReservation_AddingRoomService_AddsRoomServiceToListAndTotalPrice()
    {
        //provera da je lista prazna pre nego sto dodam
        int testResId = 2;

        var resultGet = await _controllerReservation.GetReservationById(testResId);
        var okResultGet = resultGet as OkObjectResult;
        var existingReservation = okResultGet.Value as Reservation;
        var roomServicesList = existingReservation.RoomServices;
        var totalPrice = existingReservation.TotalPrice;//120 soba x2 240 i es 20 x2 40 valjda 280
        decimal expectedTotal = 280m;

        Assert.Multiple(() =>
        {
            Assert.That(totalPrice, Is.EqualTo(expectedTotal));
            Assert.That(roomServicesList, Is.Empty);
        });

        ReservationDTO reservationDTO = new ReservationDTO
        { //ostali param su isti samo dodajemo rs u praznu listu
            //ReservationID = 2, //da znam koju menjam
            RoomNumber = 123,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3),
            RoomServiceIDs = { 1 }//kosta 10
        };

        var result = await _controllerReservation.UpdateReservation(testResId, reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Has.Property("Value").EqualTo($"Reservation with ID {testResId} updated successfully."));
        });

        //posto es ne postoji u rez koja se salje u update on se izbacuje taman provera da li i to radi 
        //tkd 240 soba i 10 za rs 250

        //provera je l dodata u listu i uracunata u cenu 
        var resultGetAfter = await _controllerReservation.GetReservationById(testResId);
        var okResultGetAfter = resultGetAfter as OkObjectResult;
        var existingReservationAfter = okResultGetAfter.Value as Reservation;
        var roomServicesListAfter = existingReservationAfter.RoomServices;
        var totalPriceAfter = existingReservationAfter.TotalPrice;//120 soba x2 240 i es 20 x2 40 valjda 280
        decimal expectedTotalAfter = 250m;

        Assert.Multiple(() =>
        {
            Assert.That(totalPriceAfter, Is.EqualTo(expectedTotalAfter));
            Assert.That(roomServicesListAfter.Count, Is.EqualTo(1));
            Assert.That(roomServicesListAfter, Has.Some.Matches<RoomService>(r => r.RoomServiceID == 1));
        });

        //jedina rezervacija za ovu sobu 

    }

    [Test]
    public async Task UpdateReservation_RemovingExtraService_RemovesReservationFromExtraServiceReservations()
    {
        int testResId = 2;
        //PROVERA JE L BILO U LISTI ES PRE TOGA
        var resultEs = await _controllerExtraService.GetReservationsByExtraServiceId(1);
        var resultEsReservations = resultEs as OkObjectResult;
        var reservationsList = resultEsReservations.Value as List<Reservation>;
        Assert.Multiple(() =>
        {
            //Assert.That(reservationsListAfter.Count, Is.EqualTo(1));
            //reservation with id 1 is removed and now it only has reservation with id 2
            Assert.That(reservationsList, Has.Some.Matches<Reservation>(r => r.ReservationID == 2));
        });

        ReservationDTO reservationDTO = new ReservationDTO
        { //ostali param su isti samo dodajemo rs u praznu listu
            //ReservationID = 2, //da znam koju menjam
            RoomNumber = 123,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3),
            RoomServiceIDs = { 1 }//kosta 10
        };

        var result = await _controllerReservation.UpdateReservation(testResId, reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Has.Property("Value").EqualTo($"Reservation with ID {testResId} updated successfully."));
        });


        //PROVERA JE L IZB REZ IZ ES KOJ JE OBRISAN
        var resultEsAfter = await _controllerExtraService.GetReservationsByExtraServiceId(1);
        var resultEsReservationsAfter = resultEsAfter as OkObjectResult;
        var reservationsListAfter = resultEsReservationsAfter.Value as List<Reservation>;
        Assert.Multiple(() =>
        {
            //Assert.That(reservationsListAfter.Count, Is.EqualTo(1));
            //reservation with id 1 is removed and now it only has reservation with id 2
            Assert.That(reservationsListAfter, Has.None.Matches<Reservation>(r => r.ReservationID == testResId));
        });
    }

    [Test]
    public async Task UpdateReservation_RemovingRoomService_RemovesReservationFromRoomServiceReservations()
    {
        int testResId = 1;
        //uklanjamo rs 1 
        //PROVERA JE L BILO U LISTI rS PRE TOGA
        var resultRs = await _controllerRoomService.GetReservationsByRoomServiceId(1);
        var resultRsReservations = resultRs as OkObjectResult;
        var reservationsList = resultRsReservations.Value as List<Reservation>;
        Assert.Multiple(() =>
        {
            //Assert.That(reservationsListAfter.Count, Is.EqualTo(1));
            //reservation with id 1 is removed and now it only has reservation with id 2
            Assert.That(reservationsList, Has.Some.Matches<Reservation>(r => r.ReservationID == testResId));
        });

        ReservationDTO reservationDTO = new ReservationDTO
        { //rez 1
            RoomNumber = 699,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3),
        };


        var result = await _controllerReservation.UpdateReservation(testResId, reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Has.Property("Value").EqualTo($"Reservation with ID {testResId} updated successfully."));
        });


        //PROVERA JE L IZB REZ IZ rS KOJ JE OBRISAN
        var resultRsAfter = await _controllerRoomService.GetReservationsByRoomServiceId(1);
        var resultRsReservationsAfter = resultRsAfter as OkObjectResult;
        var reservationsListAfter = resultRsReservationsAfter.Value as List<Reservation>;
        Assert.Multiple(() =>
        {
            //Assert.That(reservationsListAfter.Count, Is.EqualTo(1));
            //reservation with id 1 is removed and now it only has reservation with id 2
            Assert.That(reservationsListAfter, Has.None.Matches<Reservation>(r => r.ReservationID == testResId));
        });
    }


    //da li je potrebna jedna provera kad je lista prazna jedna kad vec ima neki rs
    //ova ce da bude za kad vec postoji neki es u listi
    [Test]
    public async Task UpdateReservation_AddingExtraService_AddsExtraServiceToListAndTotalPrice()
    {
        //vex ima es od 20 dodajemo od 25 dva dana 

        //provera da je u listi 1 es pre nego sto dodam
        int testResId = 2;

        var resultGet = await _controllerReservation.GetReservationById(testResId);
        var okResultGet = resultGet as OkObjectResult;
        var existingReservation = okResultGet.Value as Reservation;
        var extraServicesList = existingReservation.ExtraServices;
        var totalPrice = existingReservation.TotalPrice;//120 soba x2 240 i es 20 x2 40 valjda 280
        decimal expectedTotal = 280m;

        Assert.Multiple(() =>
        {
            Assert.That(totalPrice, Is.EqualTo(expectedTotal));
            Assert.That(extraServicesList.Count, Is.EqualTo(1));
            Assert.That(extraServicesList, Has.Some.Matches<ExtraService>(es => es.ExtraServiceID == 1));
        });

        ReservationDTO reservationDTO = new ReservationDTO
        { //ostali param su isti samo dodajemo rs u praznu listu
            //ReservationID = 2, //da znam koju menjam
            RoomNumber = 123,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3),
            ExtraServiceIDs = { 1, 2 }//20 i 25
        };

        var result = await _controllerReservation.UpdateReservation(testResId, reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Has.Property("Value").EqualTo($"Reservation with ID {testResId} updated successfully."));
        });

        //posto es ne postoji u rez koja se salje u update on se izbacuje taman provera da li i to radi 
        //tkd 240 soba i 10 za rs 250

        //provera je l dodata u listu i uracunata u cenu 
        var resultGetAfter = await _controllerReservation.GetReservationById(testResId);
        var okResultGetAfter = resultGetAfter as OkObjectResult;
        var existingReservationAfter = okResultGetAfter.Value as Reservation;
        var extraServicesListAfter = existingReservationAfter.ExtraServices;
        var totalPriceAfter = existingReservationAfter.TotalPrice;//120 soba x2 240 i es 20 x2 40 i es 25 x2 50 valjda 330
        decimal expectedTotalAfter = 330m;

        Assert.Multiple(() =>
        {
            Assert.That(totalPriceAfter, Is.EqualTo(expectedTotalAfter));
            Assert.That(extraServicesListAfter.Count, Is.EqualTo(2));
            Assert.That(extraServicesListAfter, Has.Some.Matches<ExtraService>(r => r.ExtraServiceID == 1));
            Assert.That(extraServicesListAfter, Has.Some.Matches<ExtraService>(r => r.ExtraServiceID == 2));
        });

        //jedina rezervacija za ovu sobu 

    }

    [Test]//prvo proveri da li nema tu rez pa dodaj pa proveri je l ima
    public async Task UpdateReservation_WithExtraService_AddsReservationToExtraServiceReservations()
    {
        int testResId = 2;
        int esId = 2;
        var resultEs = await _controllerExtraService.GetReservationsByExtraServiceId(esId);
        var resultEsReservations = resultEs as OkObjectResult;
        var reservationsList = resultEsReservations.Value as List<Reservation>;
        Assert.That(reservationsList, Is.Empty);

        ReservationDTO reservationDTO = new ReservationDTO
        {
            //ReservationID = 2, //da znam koju menjam
            RoomNumber = 123,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3),
            ExtraServiceIDs = { esId }
        };

        var result = await _controllerReservation.UpdateReservation(testResId, reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Has.Property("Value").EqualTo($"Reservation with ID {testResId} updated successfully."));
        });

        //provera je l dodata u listu
        var resultEsAfter = await _controllerExtraService.GetReservationsByExtraServiceId(esId);
        var resultEsReservationsAfter = resultEsAfter as OkObjectResult;
        var reservationsListAfter = resultEsReservationsAfter.Value as List<Reservation>;
        Assert.Multiple(() =>
        {
            Assert.That(reservationsListAfter.Count, Is.EqualTo(1));
            //reservation with id 1 is removed and now it only has reservation with id 2
            Assert.That(reservationsListAfter, Has.Some.Matches<Reservation>(r => r.GuestID == reservationDTO.GuestID &&
                                                                                  r.RoomNumber == reservationDTO.RoomNumber &&
                                                                                  r.CheckInDate == reservationDTO.CheckInDate &&
                                                                                  r.CheckOutDate == reservationDTO.CheckOutDate));
        });

        


    }

    [Test]
    public async Task UpdateReservation_WithRoomService_AddsReservationToRoomServiceReservations()
    {
        //int existingId = 1;
        int testResId = 2;
        int rsId = 2;
        var resultRs = await _controllerRoomService.GetReservationsByRoomServiceId(rsId);
        var resultRsReservations = resultRs as OkObjectResult;
        var reservationsList = resultRsReservations.Value as List<Reservation>;
        Assert.That(reservationsList, Is.Empty);

        ReservationDTO reservationDTO = new ReservationDTO
        {
            //ReservationID = 2, //da znam koju menjam
            RoomNumber = 123,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3),
            RoomServiceIDs = { rsId }
        };

        var result = await _controllerReservation.UpdateReservation(testResId, reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Has.Property("Value").EqualTo($"Reservation with ID {testResId} updated successfully."));
        });

        //proveri je l je sad ima

        var resultRsAfter = await _controllerRoomService.GetReservationsByRoomServiceId(rsId);
        var resultRsReservationsAfter = resultRsAfter as OkObjectResult;
        var reservationsListAfter = resultRsReservationsAfter.Value as List<Reservation>;
        Assert.Multiple(() =>
        {
            Assert.That(reservationsListAfter.Count, Is.EqualTo(1));
            Assert.That(reservationsListAfter, Has.Some.Matches<Reservation>(r => r.GuestID == reservationDTO.GuestID &&
                                                                                  r.RoomNumber == reservationDTO.RoomNumber &&
                                                                                  r.CheckInDate == reservationDTO.CheckInDate &&
                                                                                  r.CheckOutDate == reservationDTO.CheckOutDate));
        });
    }

    [Test]//menjamo rez 2 soba 123 u sobu 500
    public async Task UpdateReservation_AddsReservationToNewRoomReservationsAndRemovesFromPrevious()
    {
        int resId = 2;
        int newRoomNum = 500;
        int currentRoomNum = 123;

        //provera da je u listi trenutne sobe 
        var resultRoom = await _controllerRoom.GetRoom(currentRoomNum);
        var resultRoomOk = resultRoom as OkObjectResult;
        var roomObject = resultRoomOk.Value as RoomDTO;
        var reservationsList = roomObject.Reservations;

        Assert.That(reservationsList, Has.Some.Matches<ReservationDTO>(r => r.ReservationID == resId));
        // Assert.Multiple(() =>
        // {
        //     Assert.That(reservationsList.Count, Is.EqualTo(1));

        // });

        //provera da nije u listi buduce sobe
        var resultRoom2 = await _controllerRoom.GetRoom(newRoomNum);
        var resultRoomOk2 = resultRoom2 as OkObjectResult;
        var roomObject2 = resultRoomOk2.Value as RoomDTO;
        var reservationsList2 = roomObject2.Reservations;


        Assert.That(reservationsList2, Has.None.Matches<ReservationDTO>(r => r.ReservationID == resId));
        // Assert.Multiple(() =>
        // {
        //     Assert.That(reservationsList2.Count, Is.Empty);
        // });

        ReservationDTO reservation2 = new ReservationDTO
        {
            //ReservationID = 2,
            RoomNumber = newRoomNum,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3),
            ExtraServiceIDs = { 1 }
        };


        var result = await _controllerReservation.UpdateReservation(resId, reservation2);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Has.Property("Value").EqualTo($"Reservation with ID {resId} updated successfully."));
        });

        //provera da je nema vise u staroj sobi
        var resultRoomAfter = await _controllerRoom.GetRoom(currentRoomNum);
        var resultRoomOkAfter = resultRoomAfter as OkObjectResult;
        var roomObjectAfter = resultRoomOkAfter.Value as RoomDTO;
        var reservationsListAfter = roomObjectAfter.Reservations;

         Assert.That(reservationsListAfter.All(r => r.ReservationID != resId));
        // Assert.Multiple(() =>
        // {
        //     Assert.That(reservationsListAfter.Count, Is.EqualTo(1));
        //     //reservation with id 1 is removed and now it only has reservation with id 2

        // });

        //provera da je ima u novoj
        var resultRoomAfter2 = await _controllerRoom.GetRoom(newRoomNum);
        var resultRoomOkAfter2 = resultRoomAfter2 as OkObjectResult;
        var roomObjectAfter2 = resultRoomOkAfter2.Value as RoomDTO;
        var reservationsListAfter2 = roomObjectAfter2.Reservations;

        Assert.That(reservationsListAfter2, Has.Some.Matches<ReservationDTO>(r => r.ReservationID == resId));
    }

    [Test]//menjamo rez 2 "1234512345123" -> "1234567890123" bila anita bice mila
    public async Task UpdateReservation_AddsReservationToNewGuestReservationsAndRemovesFromPrevious()
    {
        int resId = 2;
        string currentGuestId = "1234512345123";
        string newGuestId = "1234567890123";

        //provera da je u listi trenutnog gosta
        var resultGuest = await _controllerGuest.GetGuestByJMBG(currentGuestId);
        var resultGuestOk = resultGuest as OkObjectResult;
        var guestObject = resultGuestOk.Value as GuestDTO;
        var reservationsList = guestObject.Reservations;

        Assert.That(reservationsList, Has.Some.Matches<ReservationDTO>(r => r.ReservationID == resId));

        //provera da nije u listi buduceg 
        var resultGuest2 = await _controllerGuest.GetGuestByJMBG(newGuestId);
        var resultGuestOk2 = resultGuest2 as OkObjectResult;
        var guestObject2 = resultGuestOk2.Value as GuestDTO;
        var reservationsList2 = guestObject2.Reservations;

        Assert.That(reservationsList2, Has.None.Matches<ReservationDTO>(r => r.ReservationID == resId));

        //update
        ReservationDTO reservation2 = new ReservationDTO
        {
            //ReservationID = 2,
            RoomNumber = 123,
            GuestID = newGuestId,
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3),
            ExtraServiceIDs = { 1 }
        };


        var result = await _controllerReservation.UpdateReservation(resId, reservation2);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Has.Property("Value").EqualTo($"Reservation with ID {resId} updated successfully."));
        });

        //provera da vise nije kod starog gosta

        var resultGuestAfter = await _controllerGuest.GetGuestByJMBG(currentGuestId);
        var resultGuestOkAfter = resultGuestAfter as OkObjectResult;
        var guestObjectAfter = resultGuestOkAfter.Value as GuestDTO;
        var reservationsListAfter = guestObjectAfter.Reservations;

        Assert.That(reservationsListAfter.All(r => r.ReservationID != resId));

        //provera da je kod novog gosta

        var resultGuestAfter2 = await _controllerGuest.GetGuestByJMBG(newGuestId);
        var resultGuestOkAfter2 = resultGuestAfter2 as OkObjectResult;
        var guestObjectAfter2 = resultGuestOkAfter2.Value as GuestDTO;
        var reservationsListAfter2 = guestObjectAfter2.Reservations;

        Assert.That(reservationsListAfter2, Has.Some.Matches<ReservationDTO>(r => r.ReservationID == resId));

    }

    [Test]
    public async Task UpdateReservation_WithNonExistingReservationId_ReturnsNotFound()
    {
        int nonExistingResId = 123;
        //update
        ReservationDTO reservation2 = new ReservationDTO
        {
            //ReservationID = 2,
            RoomNumber = 123,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3),
            ExtraServiceIDs = { 1 }
        };


        var result = await _controllerReservation.UpdateReservation(nonExistingResId, reservation2);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult, Has.Property("Value").EqualTo($"Reservation with ID {nonExistingResId} not found."));
        });
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}