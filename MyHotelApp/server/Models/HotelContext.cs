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
    public DbSet<RoomService> RoomServices { get; set; }
    public DbSet<Guest> Guests { get; set; }
    public DbSet<ExtraService> ExtraServices { get; set; }
    public DbSet<RoomType> RoomTypes { get; set; }


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


        modelBuilder.Entity<Reservation>()
            .HasMany(r => r.RoomServices)
            .WithMany(rs => rs.AddedToReservations)
            .UsingEntity<Dictionary<string, object>>(
                "ReservationRoomService",  // join table name
                j => j.HasOne<RoomService>().WithMany().HasForeignKey("RoomServiceID"),
                j => j.HasOne<Reservation>().WithMany().HasForeignKey("ReservationID"));

        modelBuilder.Entity<Reservation>()
            .HasMany(r => r.ExtraServices)
            .WithMany(es => es.AddedToReservations)
            .UsingEntity<Dictionary<string, object>>(
                "ExtraServiceReservation",  // join table name
                j => j.HasOne<ExtraService>().WithMany().HasForeignKey("ExtraServiceID"),
                j => j.HasOne<Reservation>().WithMany().HasForeignKey("ReservationID"));

        modelBuilder.Entity<Guest>()
            .HasMany(g => g.Reservations)
            .WithOne(r => r.Guest)
            .HasForeignKey(r => r.GuestID)
            .HasPrincipalKey(g => g.JMBG) 
            .OnDelete(DeleteBehavior.Cascade);

        // modelBuilder.Entity<Reservation>()
        //     .HasOne(r => r.Guest)
        //     .WithMany(g => g.Reservations)
        //     .HasForeignKey(r => r.GuestID)
        //     .HasPrincipalKey(g => g.JMBG) 
        //     .OnDelete(DeleteBehavior.Cascade);



    }
        

    }
