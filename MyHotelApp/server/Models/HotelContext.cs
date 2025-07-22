using Microsoft.EntityFrameworkCore;
using MyHotelApp.server.Models;

namespace MyHotelApp.server.Models;

public class HotelContext : DbContext
{
    public HotelContext(DbContextOptions<HotelContext> options)
        : base(options)
    {
    }

    public DbSet<Room> Rooms { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    //public DbSet<Employee> Employees { get; set; }
    public DbSet<RoomService> RoomServices { get; set; }
    public DbSet<Guest> Guests { get; set; }
    public DbSet<ExtraService> ExtraServices { get; set; }
}
