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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Your configuration goes here:
        modelBuilder.Entity<ExtraService>()
            .Property(e => e.ExtraServiceID)
            .ValueGeneratedOnAdd();

        // Your configuration goes here:
        modelBuilder.Entity<RoomService>()
            .Property(e => e.RoomServiceID)
            .ValueGeneratedOnAdd();
            
        // Your configuration goes here:
        modelBuilder.Entity<Reservation>()
            .Property(e => e.ReservationID)
            .ValueGeneratedOnAdd();

    }
}
