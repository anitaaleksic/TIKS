using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MyHotelApp.server.Models;

public enum RoomType
{
    Single,
    Double,
    Suite,
    Deluxe
}

public class Room
{
    [Key]
    public int RoomNumber { get; set; }
    [Required]
    public RoomType RoomType { get; set; }
    [Range(1, 5, ErrorMessage = "Capacity must be between 1 and 5.")]
    public int Capacity { get; set; } //kroz konstrukotr stavi default capacity
    [Required]
    [Range(1, 6, ErrorMessage = "Floor must be between 1 and 6.")]
    public int Floor { get; set; }
    [Required]
    public bool IsAvailable { get; set; }
    [Required]
    public int PricePerNight { get; set; }//na osnovu tipa sobe i sprata
    public string ImageUrl { get; set; }
    public List<Reservation> Reservations { get; set; }
}
