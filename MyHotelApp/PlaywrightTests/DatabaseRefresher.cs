using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHotelApp.server.Models;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using MyHotelApp.Controllers;

namespace PlaywrightTests;

public class DatabaseRefresher
{
    public static async Task AddDataAsync(HotelContext _context)
    {
        //birsanje svega 

        await _context.Database.ExecuteSqlRawAsync("DELETE FROM [ExtraServiceReservation]");

        await _context.Database.ExecuteSqlRawAsync("DELETE FROM [ReservationRoomService]");

        await _context.Database.ExecuteSqlRawAsync("DELETE FROM [ExtraServices]");
        await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT('dbo.ExtraServices', RESEED, 0)");

        await _context.Database.ExecuteSqlRawAsync("DELETE FROM [RoomServices]");
        await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT('dbo.RoomServices', RESEED, 0)");

        await _context.Database.ExecuteSqlRawAsync("DELETE FROM [Rooms]");

        await _context.Database.ExecuteSqlRawAsync("DELETE FROM [RoomTypes]");
        await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT('dbo.RoomTypes', RESEED, 0)");

        await _context.Database.ExecuteSqlRawAsync("DELETE FROM [Guests]");
        
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM [Reservations]");
        await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT('dbo.Reservations', RESEED, 0)");

        //add guests
        await _context.Guests.AddAsync(new Guest { JMBG = "0123456789012", FullName = "Lazar Živković", PhoneNumber = "+381696969696" });
        await _context.SaveChangesAsync();
        
        await _context.Guests.AddAsync(new Guest { JMBG = "1234567890123", FullName = "Ana Petrović", PhoneNumber = "+381641112223" });
        await _context.SaveChangesAsync();

        await _context.Guests.AddAsync(new Guest { JMBG = "2345678901234", FullName = "Marko Jovanović", PhoneNumber = "+381631234567" });
        await _context.SaveChangesAsync();

        await _context.Guests.AddAsync(new Guest { JMBG = "3456789012345", FullName = "Ivana Nikolić", PhoneNumber = "+381645556677" });
        await _context.SaveChangesAsync();
        
        await _context.Guests.AddAsync(new Guest { JMBG = "4567890123456", FullName = "Petar Lukić", PhoneNumber = "+381601112233" });
        await _context.SaveChangesAsync();

        await _context.Guests.AddAsync(new Guest { JMBG = "5678901234567", FullName = "Jelena Kovačević", PhoneNumber = "+381658889999" });
        await _context.SaveChangesAsync();

        await _context.Guests.AddAsync(new Guest { JMBG = "6789012345678", FullName = "Nikola Ilić", PhoneNumber = "+381607778885" });
        await _context.SaveChangesAsync();

        await _context.Guests.AddAsync(new Guest { JMBG = "7890123456789", FullName = "Milica Stanković", PhoneNumber = "+381603334455" });
        await _context.SaveChangesAsync();

        await _context.Guests.AddAsync(new Guest { JMBG = "8901234567890", FullName = "Stefan Pavlović", PhoneNumber = "+381648889999" });
        await _context.SaveChangesAsync();

        await _context.Guests.AddAsync(new Guest { JMBG = "9012345678901", FullName = "Tamara Đorđević", PhoneNumber = "+381662224445" });
        await _context.SaveChangesAsync();
        

        //add room types
        await _context.RoomTypes.AddAsync(new RoomType { Type = "Single", Capacity = 1, PricePerNight = 50.00m });
        await _context.SaveChangesAsync();

        await _context.RoomTypes.AddAsync(new RoomType { Type = "Double", Capacity = 2, PricePerNight = 80.00m });
        await _context.SaveChangesAsync();

        await _context.RoomTypes.AddAsync(new RoomType { Type = "Suite", Capacity = 3, PricePerNight = 150.00m });
        await _context.SaveChangesAsync();

        await _context.RoomTypes.AddAsync(new RoomType { Type = "Deluxe", Capacity = 4, PricePerNight = 120.00m });
        await _context.SaveChangesAsync();


 
        //add rooms 
        await _context.Rooms.AddAsync(new Room { RoomNumber = 101, RoomTypeID = 1, Floor = 1 });
        await _context.SaveChangesAsync();

        await _context.Rooms.AddAsync(new Room { RoomNumber = 102, RoomTypeID = 1, Floor = 1 });
        await _context.SaveChangesAsync();

        await _context.Rooms.AddAsync(new Room { RoomNumber = 201, RoomTypeID = 2, Floor = 2 });
        await _context.SaveChangesAsync();

        await _context.Rooms.AddAsync(new Room { RoomNumber = 202, RoomTypeID = 2, Floor = 2 });
        await _context.SaveChangesAsync();

        await _context.Rooms.AddAsync(new Room { RoomNumber = 301, RoomTypeID = 3, Floor = 3 });
        await _context.SaveChangesAsync();

        await _context.Rooms.AddAsync(new Room { RoomNumber = 302, RoomTypeID = 3, Floor = 3 });
        await _context.SaveChangesAsync();
        await _context.Rooms.AddAsync(new Room { RoomNumber = 401, RoomTypeID = 4, Floor = 4 });
        await _context.SaveChangesAsync();

        await _context.Rooms.AddAsync(new Room { RoomNumber = 402, RoomTypeID = 4, Floor = 4 });
        await _context.SaveChangesAsync();

        await _context.Rooms.AddAsync(new Room { RoomNumber = 501, RoomTypeID = 2, Floor = 5 });
        await _context.SaveChangesAsync();

        await _context.Rooms.AddAsync(new Room { RoomNumber = 502, RoomTypeID = 1, Floor = 5 });
        await _context.SaveChangesAsync();
        

        //add room services
        await _context.RoomServices.AddAsync(new RoomService { ItemName = "Breakfast", ItemPrice = 10.00m, Description = "Continental breakfast" });
        await _context.SaveChangesAsync();

        await _context.RoomServices.AddAsync(new RoomService { ItemName = "Room Cleaning", ItemPrice = 15.00m, Description = "Daily room cleaning service" });
        await _context.SaveChangesAsync();

        await _context.RoomServices.AddAsync(new RoomService { ItemName = "Laundry", ItemPrice = 12.00m, Description = "Laundry service" });
        await _context.SaveChangesAsync();

        await _context.RoomServices.AddAsync(new RoomService { ItemName = "Airport Pickup", ItemPrice = 30.00m, Description = "Transfer from airport" });
        await _context.SaveChangesAsync();

        await _context.RoomServices.AddAsync(new RoomService { ItemName = "Spa Access", ItemPrice = 25.00m, Description = "Access to spa facilities" });
        await _context.SaveChangesAsync();

        await _context.RoomServices.AddAsync(new RoomService { ItemName = "Gym Access", ItemPrice = 15.00m, Description = "Access to gym" });
        await _context.SaveChangesAsync();

        await _context.RoomServices.AddAsync(new RoomService { ItemName = "Minibar", ItemPrice = 5.00m, Description = "Minibar items" });
        await _context.SaveChangesAsync();

        await _context.RoomServices.AddAsync(new RoomService { ItemName = "Extra Towels", ItemPrice = 3.00m, Description = "Additional towels" });
        await _context.SaveChangesAsync();

        await _context.RoomServices.AddAsync(new RoomService { ItemName = "Late Checkout", ItemPrice = 40.00m, Description = "Late room checkout" });
        await _context.SaveChangesAsync();

        await _context.RoomServices.AddAsync(new RoomService { ItemName = "Breakfast in Bed", ItemPrice = 20.00m, Description = "Breakfast served in bed" });
        await _context.SaveChangesAsync();

        //add extra services
        await _context.ExtraServices.AddAsync(new ExtraService { ServiceName = "Parking Spot", Price = 10.00m, Description = "Reserved parking space" });
        await _context.SaveChangesAsync();

        await _context.ExtraServices.AddAsync(new ExtraService { ServiceName = "Restaurant Access", Price = 25.00m, Description = "Access to hotel restaurant" });
        await _context.SaveChangesAsync();

        await _context.ExtraServices.AddAsync(new ExtraService { ServiceName = "Wellness Access", Price = 30.00m, Description = "Access to wellness center" });
        await _context.SaveChangesAsync();

        await _context.ExtraServices.AddAsync(new ExtraService { ServiceName = "Airport Shuttle", Price = 20.00m, Description = "Shuttle service to airport" });
        await _context.SaveChangesAsync();

        await _context.ExtraServices.AddAsync(new ExtraService { ServiceName = "City Tour", Price = 50.00m, Description = "Guided city tour" });
        await _context.SaveChangesAsync();

        await _context.ExtraServices.AddAsync(new ExtraService { ServiceName = "Laundry Service", Price = 15.00m, Description = "Laundry for clothes" });
        await _context.SaveChangesAsync();

        await _context.ExtraServices.AddAsync(new ExtraService { ServiceName = "Breakfast Buffet", Price = 12.00m, Description = "Buffet breakfast" });
        await _context.SaveChangesAsync();

        await _context.ExtraServices.AddAsync(new ExtraService { ServiceName = "Car Rental", Price = 100.00m, Description = "Car rental service" });
        await _context.SaveChangesAsync();

        await _context.ExtraServices.AddAsync(new ExtraService { ServiceName = "WiFi Access", Price = 0.00m, Description = "Free WiFi" });
        await _context.SaveChangesAsync();

        await _context.ExtraServices.AddAsync(new ExtraService { ServiceName = "Room Decoration", Price = 40.00m, Description = "Special room decoration" });
        await _context.SaveChangesAsync();

        //add reservations 
        await _context.Reservations.AddAsync(new Reservation { RoomNumber = 101, GuestID = "1234567890123", CheckInDate = DateTime.Parse("2025-01-10"), CheckOutDate = DateTime.Parse("2025-01-14"), TotalPrice = 290.00m }); 
        await _context.SaveChangesAsync();

        await _context.Reservations.AddAsync(new Reservation { RoomNumber = 102, GuestID = "2345678901234", CheckInDate = DateTime.Parse("2025-02-05"), CheckOutDate = DateTime.Parse("2025-02-09"), TotalPrice = 415.00m }); 
        await _context.SaveChangesAsync();
        
        await _context.Reservations.AddAsync(new Reservation { RoomNumber = 201, GuestID = "3456789012345", CheckInDate = DateTime.Parse("2025-03-15"), CheckOutDate = DateTime.Parse("2025-03-21"), TotalPrice = 690.00m });
        await _context.SaveChangesAsync();
        
        await _context.Reservations.AddAsync(new Reservation { RoomNumber = 202, GuestID = "4567890123456", CheckInDate = DateTime.Parse("2025-04-01"), CheckOutDate = DateTime.Parse("2025-04-04"), TotalPrice = 540.00m });
        await _context.SaveChangesAsync();

        await _context.Reservations.AddAsync(new Reservation { RoomNumber = 301, GuestID = "5678901234567", CheckInDate = DateTime.Parse("2025-05-12"), CheckOutDate = DateTime.Parse("2025-05-14"), TotalPrice = 257.00m });
        await _context.SaveChangesAsync();

        await _context.Reservations.AddAsync(new Reservation { RoomNumber = 302, GuestID = "6789012345678", CheckInDate = DateTime.Parse("2025-06-01"), CheckOutDate = DateTime.Parse("2025-06-05"), TotalPrice = 380.00m });
        await _context.SaveChangesAsync();
        
        await _context.Reservations.AddAsync(new Reservation { RoomNumber = 401, GuestID = "7890123456789", CheckInDate = DateTime.Parse("2025-07-10"), CheckOutDate = DateTime.Parse("2025-07-14"), TotalPrice = 695.00m });
        await _context.SaveChangesAsync();

        await _context.Reservations.AddAsync(new Reservation { RoomNumber = 402, GuestID = "8901234567890", CheckInDate = DateTime.Parse("2025-08-03"), CheckOutDate = DateTime.Parse("2025-08-08"), TotalPrice = 587.00m });
        await _context.SaveChangesAsync();
        
        await _context.Reservations.AddAsync(new Reservation { RoomNumber = 501, GuestID = "9012345678901", CheckInDate = DateTime.Parse("2025-09-15"), CheckOutDate = DateTime.Parse("2025-09-17"), TotalPrice = 390.00m });
        await _context.SaveChangesAsync();

        await _context.Reservations.AddAsync(new Reservation { RoomNumber = 502, GuestID = "0123456789012", CheckInDate = DateTime.Parse("2025-10-05"), CheckOutDate = DateTime.Parse("2025-10-08"), TotalPrice = 482.00m }); 
        await _context.SaveChangesAsync();

        //add res roomServ and extraServ
        //Reservation 1 has RoomServices 1,3,5 and ExtraServices 1,2
        var reservation1 = await _context.Reservations.FindAsync(1);
        reservation1.RoomServices.AddRange(await _context.RoomServices.Where(rs => new[] { 1, 3, 5 }.Contains(rs.RoomServiceID)).ToListAsync());
        reservation1.ExtraServices.AddRange(await _context.ExtraServices.Where(es => new[] { 1, 2 }.Contains(es.ExtraServiceID)).ToListAsync());

        // Reservation 2
        var reservation2 = await _context.Reservations.FindAsync(2);
        reservation2.RoomServices.AddRange(await _context.RoomServices.Where(rs => new[] { 2, 4 }.Contains(rs.RoomServiceID)).ToListAsync());
        reservation2.ExtraServices.AddRange(await _context.ExtraServices.Where(es => new[] { 3, 4 }.Contains(es.ExtraServiceID)).ToListAsync());

        // Reservation 3
        var reservation3 = await _context.Reservations.FindAsync(3);
        reservation3.RoomServices.AddRange(await _context.RoomServices.Where(rs => new[] { 1, 6 }.Contains(rs.RoomServiceID)).ToListAsync());
        reservation3.ExtraServices.AddRange(await _context.ExtraServices.Where(es => new[] { 5, 6 }.Contains(es.ExtraServiceID)).ToListAsync());

        // Reservation 4
        var reservation4 = await _context.Reservations.FindAsync(4);
        reservation4.RoomServices.AddRange(await _context.RoomServices.Where(rs => new[] { 2, 3, 5 }.Contains(rs.RoomServiceID)).ToListAsync());
        reservation4.ExtraServices.Add(await _context.ExtraServices.FindAsync(1));

        // Reservation 5
        var reservation5 = await _context.Reservations.FindAsync(5);
        reservation5.RoomServices.Add(await _context.RoomServices.FindAsync(7));
        reservation5.ExtraServices.AddRange(await _context.ExtraServices.Where(es => new[] { 2, 7 }.Contains(es.ExtraServiceID)).ToListAsync());

        // Reservation 6
        var reservation6 = await _context.Reservations.FindAsync(6);
        reservation6.RoomServices.AddRange(await _context.RoomServices.Where(rs => new[] { 4, 8 }.Contains(rs.RoomServiceID)).ToListAsync());
        reservation6.ExtraServices.AddRange(await _context.ExtraServices.Where(es => new[] { 8, 9 }.Contains(es.ExtraServiceID)).ToListAsync());

        // Reservation 7
        var reservation7 = await _context.Reservations.FindAsync(7);
        reservation7.RoomServices.Add(await _context.RoomServices.FindAsync(6));
        reservation7.ExtraServices.Add(await _context.ExtraServices.FindAsync(10));

        // Reservation 8
        var reservation8 = await _context.Reservations.FindAsync(8);
        reservation8.RoomServices.AddRange(await _context.RoomServices.Where(rs => new[] { 1, 2 }.Contains(rs.RoomServiceID)).ToListAsync());
        reservation8.ExtraServices.AddRange(await _context.ExtraServices.Where(es => new[] { 3, 4 }.Contains(es.ExtraServiceID)).ToListAsync());

        // Reservation 9
        var reservation9 = await _context.Reservations.FindAsync(9);
        reservation9.RoomServices.AddRange(await _context.RoomServices.Where(rs => new[] { 9, 10 }.Contains(rs.RoomServiceID)).ToListAsync());
        reservation9.ExtraServices.AddRange(await _context.ExtraServices.Where(es => new[] { 5, 6 }.Contains(es.ExtraServiceID)).ToListAsync());

        // Reservation 10
        var reservation10 = await _context.Reservations.FindAsync(10);
        reservation10.RoomServices.AddRange(await _context.RoomServices.Where(rs => new[] { 5, 7 }.Contains(rs.RoomServiceID)).ToListAsync());
        reservation10.ExtraServices.AddRange(await _context.ExtraServices.Where(es => new[] { 7, 8, 9, 10 }.Contains(es.ExtraServiceID)).ToListAsync());

        await _context.SaveChangesAsync();


    }


}