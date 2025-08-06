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
public class ReservationController_DeleteReservation_Tests
{
    private static HotelContext _context;
    private static ReservationController _controllerReservation;
    private static RoomServiceController _controllerRoomService;
    private static ExtraServiceController _controllerExtraService;
    private static RoomController _controllerRoom;
    private static GuestController _controllerGuest;
    private Reservation _reservation;
    //private Room _room;
    //private RoomService _roomService;

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

        //ZAKLJUCAK 
        //moraju da se dodaju sobe inace nece da napravi rezervaciju 
        //fja GetReservationByGuest mora da vraca listu a ne IActionResult 
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
        //,
        //    Reservations = { _reservation }

        _context.Rooms.Add(room);
        _context.SaveChanges();

        RoomService roomService= new RoomService
        {
            RoomServiceID = 1,
            ItemName = "Breakfast",
            ItemPrice = 10m,
            Description = "Continental breakfast"
        };
        _context.RoomServices.Add(roomService);

        _context.SaveChanges();

        ExtraService es = new ExtraService
        {
            ExtraServiceID = 1,
            ServiceName = "Parking Spot",
            Price = 10m,
            Description = "Reserved parking space"
        };

        _context.ExtraServices.Add(es);

        _context.SaveChanges();

        _reservation = new Reservation
        {
            ReservationID = 1,
            RoomNumber = 123,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3),
            TotalPrice = 320.00m,
            RoomServices = { roomService },
            ExtraServices = { es }
        };
        _context.Reservations.Add(_reservation);
        _context.SaveChanges();

        _context.Reservations.Add(new Reservation
        {
            ReservationID = 2,
            RoomNumber = 124,
            GuestID = "1234512345123",
            CheckInDate = new DateTime(2025, 9, 1),
            CheckOutDate = new DateTime(2025, 9, 3),
            TotalPrice = 320.00m,
            RoomServices = { roomService },
            ExtraServices = { es }
        });

        _context.SaveChanges();

    }

    [Test]
    public async Task DeleteReservation_WithNonExistingId_ReturnsNotFound()
    {
        int nonExistingId = 12;
        var result = await _controllerReservation.DeleteReservation(nonExistingId);
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());

        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Has.Property("Value").EqualTo($"Reservation with ID {nonExistingId} not found."));
    }

    [Test]
    public async Task DeleteReservation_WithExistingId_DeletesReservation()
    {
        int existingId = 1;
        var result = await _controllerReservation.DeleteReservation(existingId);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Has.Property("Value").EqualTo($"Reservation with ID {existingId} deleted successfully."));

        //making sure its removed from the db
        var removedResult = await _controllerReservation.GetReservationById(existingId);
        Assert.That(removedResult, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = removedResult as NotFoundObjectResult;
        Assert.That(notFoundResult, Has.Property("Value").EqualTo($"Reservation with ID {existingId} not found."));
    }

    [Test]
    public async Task DeleteReservation_ReservedRoomBecomesAvailable()
    {
        var result = await _controllerReservation.IsRoomAvailable(_reservation.RoomNumber, _reservation.CheckInDate, _reservation.CheckOutDate);
        var roomAvailability = result as OkObjectResult;
        Assert.That(roomAvailability.Value, Is.False);

        //delete reservation sve isto kao sa brisanje sa existingId
        int existingId = 1;
        var resultDelete = await _controllerReservation.DeleteReservation(existingId);
        Assert.That(resultDelete, Is.InstanceOf<OkObjectResult>());

        var okResult = resultDelete as OkObjectResult;
        Assert.That(okResult, Has.Property("Value").EqualTo($"Reservation with ID {existingId} deleted successfully."));

        //making sure its removed from the db
        var removedResult = await _controllerReservation.GetReservationById(existingId);
        Assert.That(removedResult, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = removedResult as NotFoundObjectResult;
        Assert.That(notFoundResult, Has.Property("Value").EqualTo($"Reservation with ID {existingId} not found."));

        //nez je l treba sve ovo iznad 

        //sad je rez sig obrisana proveravamo availability za sobu
        var resultAfter = await _controllerReservation.IsRoomAvailable(_reservation.RoomNumber, _reservation.CheckInDate, _reservation.CheckOutDate);
        var roomAvailabilityAfter = resultAfter as OkObjectResult;
        Assert.That(roomAvailabilityAfter.Value, Is.True);

    }

    [Test]
    public async Task DeleteReservation_WithRoomService_RemovesReservationFromRoomServicesReservations()
    {
        int existingId = 1;
        //pull added roomServices and check if they exist
        // int existingId = 1;
        // var result = await _controllerReservation.GetReservationById(existingId);
        // var resultReservation = result as OkObjectResult;
        // var reservation = resultReservation.Value as Reservation;
        // Assert.That(reservation.RoomServices, Is.Not.Empty);
        // Assert.That(reservation.RoomServices.Count, Is.EqualTo(1));

        //trebalo je da proveris je l postoji rezervacija u listi roomServica glupaco
        int rsId = 1;
        var resultRs = await _controllerRoomService.GetReservationsByRoomServiceId(rsId);
        var resultRsReservations = resultRs as OkObjectResult;
        var reservationsList = resultRsReservations.Value as List<Reservation>;
        Assert.Multiple(() =>
        {
            Assert.That(reservationsList, Is.Not.Empty);
            Assert.That(reservationsList, Has.Some.Matches<Reservation>(r => r.ReservationID == existingId));
        });
        
        //ako ga sadrzi tjt sad ga brisemo pa proveravamo da li ga i dalje sadrzi

        //delete reservation sve isto kao sa brisanje sa existingId

        var resultDelete = await _controllerReservation.DeleteReservation(existingId);
        var okResult = resultDelete as OkObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(resultDelete, Is.InstanceOf<OkObjectResult>());
            Assert.That(okResult, Has.Property("Value").EqualTo($"Reservation with ID {existingId} deleted successfully."));
        });

        //making sure its removed from the db
        var removedResult = await _controllerReservation.GetReservationById(existingId);
        var notFoundResult = removedResult as NotFoundObjectResult;
        Assert.Multiple(() => 
        {            
            Assert.That(removedResult, Is.InstanceOf<NotFoundObjectResult>());
            Assert.That(notFoundResult, Has.Property("Value").EqualTo($"Reservation with ID {existingId} not found."));
        });

        //nez je l treba sve ovo iznad 

        //sad je rez sig obrisana proveravamo da li roomServices vise nemaju vezu sa tom rezervacijom
        //ubacila sam dve rezervacije da bi lista sad imala count 1 pa da proverim da nije isti key 
        //da je samo vezana za jednu rezervaciju sad bi vracalo praznu listu pa da prosaramo malo 
        var resultRsAfter = await _controllerRoomService.GetReservationsByRoomServiceId(rsId);
        var resultRsReservationsAfter = resultRsAfter as OkObjectResult;
        var reservationsListAfter = resultRsReservationsAfter.Value as List<Reservation>;
        Assert.Multiple(() =>
        {
            Assert.That(reservationsListAfter.Count, Is.EqualTo(1));
            //reservation with id 1 is removed and now it only has reservation with id 2
            Assert.That(reservationsList, Has.Some.Matches<Reservation>(r => r.ReservationID == 2));
            Assert.That(reservationsList, Has.None.Matches<Reservation>(r => r.ReservationID == existingId));
        });
    }

    //ista provera za extra service kao za room service
    [Test]
    public async Task DeleteReservation_WithExtraService_RemovesReservationFromExtraServicesReservations()
    {
        int existingId = 1;
        int esId = 1;
        var resultEs = await _controllerExtraService.GetReservationsByExtraServiceId(esId);
        var resultEsReservations = resultEs as OkObjectResult;
        var reservationsList = resultEsReservations.Value as List<Reservation>;
        Assert.Multiple(() =>
        {
            Assert.That(reservationsList, Is.Not.Empty);
            Assert.That(reservationsList, Has.Some.Matches<Reservation>(r => r.ReservationID == existingId));
        });
        
        //ako ga sadrzi tjt sad ga brisemo pa proveravamo da li ga i dalje sadrzi

        //delete reservation sve isto kao sa brisanje sa existingId

        var resultDelete = await _controllerReservation.DeleteReservation(existingId);
        var okResult = resultDelete as OkObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(resultDelete, Is.InstanceOf<OkObjectResult>());
            Assert.That(okResult, Has.Property("Value").EqualTo($"Reservation with ID {existingId} deleted successfully."));
        });

        //making sure its removed from the db
        var removedResult = await _controllerReservation.GetReservationById(existingId);
        var notFoundResult = removedResult as NotFoundObjectResult;
        Assert.Multiple(() => 
        {            
            Assert.That(removedResult, Is.InstanceOf<NotFoundObjectResult>());
            Assert.That(notFoundResult, Has.Property("Value").EqualTo($"Reservation with ID {existingId} not found."));
        });

        //nez je l treba sve ovo iznad 

        var resultEsAfter = await _controllerExtraService.GetReservationsByExtraServiceId(esId);
        var resultEsReservationsAfter = resultEsAfter as OkObjectResult;
        var reservationsListAfter = resultEsReservationsAfter.Value as List<Reservation>;
        Assert.Multiple(() =>
        {
            Assert.That(reservationsListAfter.Count, Is.EqualTo(1));
            //reservation with id 1 is removed and now it only has reservation with id 2
            Assert.That(reservationsList, Has.Some.Matches<Reservation>(r => r.ReservationID == 2));
            Assert.That(reservationsList, Has.None.Matches<Reservation>(r => r.ReservationID == existingId));
        });
    }

    [Test]//proba prvo bez da ubacim rezervaciju u sobu kad je kreiram da vidim je l se doda sama kad u rezervaciju stavim broj sobe
    //DA NE TREBA DA UBACIM REZERVACIJU I NE MOZE SVAKAKO JER SE ONA KREIRA NA KRAJU SA SVIM VEZAMA
    //i proba bez da pravim getRoomReservations nego da ga iskopam iz getRoom  
    public async Task DeleteReservation_RemovesReservationFromRoomReservations()
    {
        int roomId = 123;
        int reservationId = 1;

        var resultRoom = await _controllerRoom.GetRoom(roomId);
        var resultRoomOk = resultRoom as OkObjectResult;
        var roomObject = resultRoomOk.Value as Room;
        var reservationsList = roomObject.Reservations;

        Assert.Multiple(() =>
        {
            Assert.That(reservationsList, Is.Not.Empty);
            Assert.That(reservationsList, Has.Some.Matches<Reservation>(r => r.ReservationID == reservationId));
        });

        //ako ga sadrzi tjt sad ga brisemo pa proveravamo da li ga i dalje sadrzi

        //delete reservation sve isto kao sa brisanje sa existingId

        var resultDelete = await _controllerReservation.DeleteReservation(reservationId);
        var okResult = resultDelete as OkObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(resultDelete, Is.InstanceOf<OkObjectResult>());
            Assert.That(okResult, Has.Property("Value").EqualTo($"Reservation with ID {reservationId} deleted successfully."));
        });

        //making sure its removed from the db
        var removedResult = await _controllerReservation.GetReservationById(reservationId);
        var notFoundResult = removedResult as NotFoundObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(removedResult, Is.InstanceOf<NotFoundObjectResult>());
            Assert.That(notFoundResult, Has.Property("Value").EqualTo($"Reservation with ID {reservationId} not found."));
        });

        //nez je l treba sve ovo iznad 

        var resultRoomAfter = await _controllerRoom.GetRoom(roomId);
        var resultRoomOkAfter = resultRoomAfter as OkObjectResult;
        var roomObjectAfter = resultRoomOkAfter.Value as Room;
        var reservationsListAfter = roomObject.Reservations;

        Assert.Multiple(() =>
        {
            Assert.That(reservationsListAfter.Count, Is.EqualTo(0));
            //ili jedno ili drugo 
            Assert.That(reservationsList, Has.None.Matches<Reservation>(r => r.ReservationID == reservationId));
        });

    }

    [Test]
    public async Task DeleteReservation_RemovesReservationFromGuestReservations()
    {
        string guestId = "1234512345123";
        int reservationId1 = 1;
        int reservationId2 = 2;

        var resultGuest = await _controllerGuest.GetGuestByJMBG(guestId);
        var resultGuestOk = resultGuest as OkObjectResult;
        var guestObject = resultGuestOk.Value as Guest;
        var reservationsList = guestObject.Reservations;

        Assert.Multiple(() =>
        {
            Assert.That(reservationsList.Count, Is.EqualTo(2));
            Assert.That(reservationsList, Has.Some.Matches<Reservation>(r => r.ReservationID == reservationId1));
            Assert.That(reservationsList, Has.Some.Matches<Reservation>(r => r.ReservationID == reservationId2));
        });

        //ako ga sadrzi tjt sad ga brisemo pa proveravamo da li ga i dalje sadrzi

        //delete reservation sve isto kao sa brisanje sa existingId

        var resultDelete = await _controllerReservation.DeleteReservation(reservationId1);
        var okResult = resultDelete as OkObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(resultDelete, Is.InstanceOf<OkObjectResult>());
            Assert.That(okResult, Has.Property("Value").EqualTo($"Reservation with ID {reservationId1} deleted successfully."));
        });

        //making sure its removed from the db
        var removedResult = await _controllerReservation.GetReservationById(reservationId1);
        var notFoundResult = removedResult as NotFoundObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(removedResult, Is.InstanceOf<NotFoundObjectResult>());
            Assert.That(notFoundResult, Has.Property("Value").EqualTo($"Reservation with ID {reservationId1} not found."));
        });

        //nez je l treba sve ovo iznad 
        
        var resultGuestAfter = await _controllerGuest.GetGuestByJMBG(guestId);
        var resultGuestOkAfter = resultGuestAfter as OkObjectResult;
        var guestObjectAfter = resultGuestOkAfter.Value as Guest;
        var reservationsListAfter = guestObjectAfter.Reservations;

        Assert.Multiple(() =>
        {
            Assert.That(reservationsList.Count, Is.EqualTo(1));
            Assert.That(reservationsList, Has.None.Matches<Reservation>(r => r.ReservationID == reservationId1));
            Assert.That(reservationsList, Has.Some.Matches<Reservation>(r => r.ReservationID == reservationId2));
        });
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}