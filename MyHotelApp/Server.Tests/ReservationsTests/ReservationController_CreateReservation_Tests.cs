using Microsoft.EntityFrameworkCore;
using MyHotelApp.server.Models;
using MyHotelApp.Controllers;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Collections.Generic;
using Microsoft.Identity.Client;

namespace ReservationTests;

[TestFixture]
public class ReservationController_CreateReservation_Tests
{
    private static HotelContext _context;
    private static ReservationController _controllerReservation;
    private static RoomServiceController _controllerRoomService;
    private static ExtraServiceController _controllerExtraService;
    private static RoomController _controllerRoom;
    private static GuestController _controllerGuest;
    //private Reservation _reservation;
    //private Room _room;
    private RoomService _roomService;
    private ExtraService _extraService;

    [SetUp]
    public void SetUp()
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

        Reservation reservation = new Reservation
        {
            ReservationID = 1,
            RoomNumber = 699,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3)
        };
        _context.Reservations.Add(reservation);
        _context.SaveChanges();

        // _context.Reservations.Add(new Reservation
        // {
        //     ReservationID = 2,
        //     RoomNumber = 124,
        //     GuestID = "1234512345123",
        //     CheckInDate = new DateTime(2025, 9, 1),
        //     CheckOutDate = new DateTime(2025, 9, 3),
        //     TotalPrice = 320.00m,
        //     RoomServices = { roomService },
        //     ExtraServices = { es }
        // });

        // _context.SaveChanges();

    }

    [Test] //add to all Create tests
    public async Task CreateReservation_WithModelStateInvalid_ReturnsBadRequest()
    {
        // Arrange
        _controllerReservation.ModelState.AddModelError("error", "some model state error");
        var reservationDTO = new ReservationDTO();

        var result = await _controllerReservation.CreateReservation(reservationDTO);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task CreateReservation_WithRoomNumberTooSmall_ReturnsBadRequest()
    {
        var smallRoomNum = 100;
        ReservationDTO reservationDTO = new ReservationDTO
        {
            RoomNumber = smallRoomNum,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3),
            TotalPrice = 120.00m
        };

        var result = await _controllerReservation.CreateReservation(reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badReqObj = result as BadRequestObjectResult;
            Assert.That(badReqObj, Has.Property("Value").EqualTo("Room number must be between 101 and 699."));
        });        
    }

    [Test]
    public async Task CreateReservation_WithRoomNumberTooLarge_ReturnsBadRequest()
    {
        var largeRoomNum = 700;
        ReservationDTO reservationDTO = new ReservationDTO
        {
            RoomNumber = largeRoomNum,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3),
            TotalPrice = 120.00m
        };

        var result = await _controllerReservation.CreateReservation(reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badReqObj = result as BadRequestObjectResult;
            Assert.That(badReqObj, Has.Property("Value").EqualTo("Room number must be between 101 and 699."));
        });        
    }

    [Test]
    public async Task CreateReservation_WithRoomNumberOnLimit_CreatesReservation()
    {
        var limitRoomNum = 699;
        ReservationDTO reservationDTO = new ReservationDTO
        {
            RoomNumber = limitRoomNum,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 10, 1),
            CheckOutDate = new DateTime(2025, 10, 3),
            TotalPrice = 120.00m
        };

        var result = await _controllerReservation.CreateReservation(reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Has.Property("Value").EqualTo($"Reservation created successfully for room {limitRoomNum}."));
        });        
    }

    [Test]
    public async Task CreateReservation_WithNonExistingRoom_ReturnsNotFound()
    {
        var nonExistingRoomNum = 200;
        ReservationDTO reservationDTO = new ReservationDTO
        {
            RoomNumber = nonExistingRoomNum,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3),
            TotalPrice = 120.00m
        };

        var result = await _controllerReservation.CreateReservation(reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
            var notFoundObj = result as NotFoundObjectResult;
            Assert.That(notFoundObj, Has.Property("Value").EqualTo($"Room with number {nonExistingRoomNum} does not exist."));
        });        
    }

    [Test]
    public async Task CreateReservation_WithNonExistingGuest_ReturnsNotFound()
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

        var result = await _controllerReservation.CreateReservation(reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
            var notFoundObj = result as NotFoundObjectResult;
            Assert.That(notFoundObj, Has.Property("Value").EqualTo($"Guest with ID {nonExistingGuest} does not exist."));
        });        
    }

    [Test]
    public async Task CreateReservation_WithGuestIDInvalid_ReturnsBadRequest()
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

        var result = await _controllerReservation.CreateReservation(reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badReqObj = result as BadRequestObjectResult;
            Assert.That(badReqObj, Has.Property("Value").EqualTo("JMBG must be exactly 13 characters long."));
        });          
    }

    [Test]
    public async Task CreateReservation_WithCheckInOutDatesInvalid_ReturnsBadRequest()
    {
        ReservationDTO reservationDTO = new ReservationDTO
        {
            RoomNumber = 123,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 2),
            CheckOutDate = new DateTime(2025, 9, 1),
            TotalPrice = 120.00m
        };

        var result = await _controllerReservation.CreateReservation(reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badReqObj = result as BadRequestObjectResult;
            Assert.That(badReqObj, Has.Property("Value").EqualTo("Check-out date must be after check-in date."));
        });        
    }

    [Test]
    public async Task CreateReservation_WithCheckInOutDatesEqual_ReturnsBadRequest()
    {
        ReservationDTO reservationDTO = new ReservationDTO
        {
            RoomNumber = 123,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 1),
            TotalPrice = 120.00m
        };

        var result = await _controllerReservation.CreateReservation(reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badReqObj = result as BadRequestObjectResult;
            Assert.That(badReqObj, Has.Property("Value").EqualTo("Check-out date must be after check-in date."));
        });        
    }

    [Test]
    public async Task CreateReservation_WithOverlapingReservationDates_ReturnsBadRequest()
    {
         ReservationDTO reservationDTO = new ReservationDTO
        {
            RoomNumber = 699,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 2),
            CheckOutDate = new DateTime(2025, 9, 4)
        };

        var result = await _controllerReservation.CreateReservation(reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badReqObj = result as BadRequestObjectResult;
            Assert.That(badReqObj, Has.Property("Value").EqualTo("The room is already reserved for the selected dates."));
        });        
    }

    [Test]
    public async Task CreateReservation_WithAlmostOverlapingReservationDates_CreatesReservation()
    {
        ReservationDTO reservationDTO = new ReservationDTO
        {
            RoomNumber = 699,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 3),
            CheckOutDate = new DateTime(2025, 9, 4)
        };

        var result = await _controllerReservation.CreateReservation(reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Has.Property("Value").EqualTo($"Reservation created successfully for room 699."));
        });       
    }

    //ove za dodavanje rs i es u price potvrdjuju da ima price i bez toga
    [Test]
    public async Task CreateReservation_WithRoomService_AddsRoomServiceToTotalPrice()
    {
        decimal expectedTotal = 250m;
        int roomNum = 500;
        ReservationDTO reservationDTO = new ReservationDTO
        {
            RoomNumber = roomNum,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 6),
            CheckOutDate = new DateTime(2025, 9, 8),
            RoomServiceIDs = { 1 }
        };

        //rezervisana soba tipa 1 (cena po noci 120) na dva dana (240) sa roomServ koj kosta 10 total = 250

        var result = await _controllerReservation.CreateReservation(reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Has.Property("Value").EqualTo($"Reservation created successfully for room {roomNum}."));
        });

        //jedina rezervacija za ovu sobu 
        var resultGet = await _controllerReservation.GetReservationsByRoom(roomNum);
        var okResultGet = resultGet as OkObjectResult;
        var createdReservationList = okResultGet.Value as List<Reservation>;
        var myReservation = createdReservationList.FirstOrDefault();

        Assert.That(myReservation.TotalPrice, Is.EqualTo(expectedTotal));
    }

    [Test]
    public async Task CreateReservation_WithExtraService_AddsExtraServiceToTotalPrice()
    {
        decimal expectedTotal = 280m;
        int roomNum = 500;
        ReservationDTO reservationDTO = new ReservationDTO
        {
            RoomNumber = roomNum,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 6),
            CheckOutDate = new DateTime(2025, 9, 8),
            ExtraServiceIDs = { 1 }
        };

        //rezervisana soba tipa 1 (cena po noci 120) na dva dana (240) sa extraServ koj kosta 20 total = 280

        var result = await _controllerReservation.CreateReservation(reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Has.Property("Value").EqualTo($"Reservation created successfully for room {roomNum}."));
        });

        //jedina rezervacija za ovu sobu 
        var resultGet = await _controllerReservation.GetReservationsByRoom(roomNum);
        var okResultGet = resultGet as OkObjectResult;
        var createdReservationList = okResultGet.Value as List<Reservation>;
        var myReservation = createdReservationList.FirstOrDefault();

        Assert.That(myReservation.TotalPrice, Is.EqualTo(expectedTotal));
    }

    //VRATI NEGDE SOBU DA POGLEDAS TOTAL PRICE --radi dobro
    //PROVERI JE L RADI KAD POSALJES REZ BEZ TOTAL PRICE -- radi moz i da se obrise svuda al ne mora
    //PROVERA JE L DODAJE TU REZ SVIMA U LISTE I GOST I SOBA I ROOMSERV I EXTRASERV ubicu se - gotovo fala gadu

    [Test]//prvo proveri da li nema tu rez pa dodaj pa proveri je l ima
    public async Task CreateReservation_WithExtraService_AddsReservationToExtraServiceReservations()
    {        
        int esId = 2;
        var resultEs = await _controllerExtraService.GetReservationsByExtraServiceId(esId);
        var resultEsReservations = resultEs as OkObjectResult;
        var reservationsList = resultEsReservations.Value as List<Reservation>;
        Assert.That(reservationsList, Is.Empty);

        int roomNum = 500;
        ReservationDTO reservationDTO = new ReservationDTO
        {
            RoomNumber = roomNum,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 11),
            CheckOutDate = new DateTime(2025, 9, 13),
            ExtraServiceIDs = { esId }
        };

        //kreiraj rez proveri je l kreirana
        var result = await _controllerReservation.CreateReservation(reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Has.Property("Value").EqualTo($"Reservation created successfully for room {roomNum}."));
        });
        
        //proveri je l je sad ima
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
    public async Task CreateReservation_WithRoomService_AddsReservationToRoomServiceReservations()
    {
        //int existingId = 1;
        int rsId = 2;
        var resultRs = await _controllerRoomService.GetReservationsByRoomServiceId(rsId);
        var resultRsReservations = resultRs as OkObjectResult;
        var reservationsList = resultRsReservations.Value as List<Reservation>;
        Assert.That(reservationsList, Is.Empty);
        
        int roomNum = 500;
        ReservationDTO reservationDTO = new ReservationDTO
        {
            RoomNumber = roomNum,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 11),
            CheckOutDate = new DateTime(2025, 9, 13),
            RoomServiceIDs = { rsId }
        };

        //kreiraj rez proveri je l kreirana
        var result = await _controllerReservation.CreateReservation(reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Has.Property("Value").EqualTo($"Reservation created successfully for room {roomNum}."));
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

    [Test]
    public async Task CreateReservation_AddsReservationToRoomReservations()
    {        
        int roomNum = 500;
        var resultRoom = await _controllerRoom.GetRoom(roomNum);
        var resultRoomOk = resultRoom as OkObjectResult;
        var roomObject = resultRoomOk.Value as RoomDTO;
        var reservationsList = roomObject.Reservations;

        Assert.That(reservationsList, Is.Empty);
        
        ReservationDTO reservationDTO = new ReservationDTO
        {
            RoomNumber = roomNum,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 11),
            CheckOutDate = new DateTime(2025, 9, 13)
        };

        //kreiraj rez proveri je l kreirana
        var result = await _controllerReservation.CreateReservation(reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Has.Property("Value").EqualTo($"Reservation created successfully for room {roomNum}."));
        });


        var resultRoomAfter = await _controllerRoom.GetRoom(roomNum);
        var resultRoomOkAfter = resultRoomAfter as OkObjectResult;
        var roomObjectAfter = resultRoomOkAfter.Value as RoomDTO;
        var reservationsListAfter = roomObjectAfter.Reservations;

        Assert.Multiple(() =>
        {
            Assert.That(reservationsListAfter.Count, Is.EqualTo(1));
            //reservation with id 1 is removed and now it only has reservation with id 2
            Assert.That(reservationsListAfter, Has.Some.Matches<ReservationDTO>(r => r.GuestID == reservationDTO.GuestID &&
                                                                                  r.RoomNumber == reservationDTO.RoomNumber &&
                                                                                  r.CheckInDate == reservationDTO.CheckInDate &&
                                                                                  r.CheckOutDate == reservationDTO.CheckOutDate));
        });
    }

    [Test]
    public async Task CreateReservation_AddsReservationToGuestReservations()
    {
        string guestId = "1234567890123";

        var resultGuest = await _controllerGuest.GetGuestByJMBG(guestId);
        var resultGuestOk = resultGuest as OkObjectResult;
        var guestObject = resultGuestOk.Value as GuestDTO;
        var reservationsList = guestObject.Reservations;

        Assert.That(reservationsList, Is.Empty);
        int roomNum = 500;

        ReservationDTO reservationDTO = new ReservationDTO
        {
            RoomNumber = roomNum,
            GuestID = guestId,
            CheckInDate = new DateTime(2025, 9, 11),
            CheckOutDate = new DateTime(2025, 9, 13)
        };

        //kreiraj rez proveri je l kreirana
        var result = await _controllerReservation.CreateReservation(reservationDTO);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Has.Property("Value").EqualTo($"Reservation created successfully for room {roomNum}."));
        });


        var resultGuestAfter = await _controllerGuest.GetGuestByJMBG(guestId);
        var resultGuestOkAfter = resultGuestAfter as OkObjectResult;
        var guestObjectAfter = resultGuestOkAfter.Value as GuestDTO;
        var reservationsListAfter = guestObjectAfter.Reservations;

        Assert.Multiple(() =>
        {
            Assert.That(reservationsListAfter.Count, Is.EqualTo(1));
            Assert.That(reservationsListAfter, Has.Some.Matches<ReservationDTO>(r => r.GuestID == reservationDTO.GuestID &&
                                                                                  r.RoomNumber == reservationDTO.RoomNumber &&
                                                                                  r.CheckInDate == reservationDTO.CheckInDate &&
                                                                                  r.CheckOutDate == reservationDTO.CheckOutDate));
        });
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}