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
    [Range(1, 999, ErrorMessage = "Room number must be between 1 and 999.")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required int RoomNumber { get; set; }
    [Required]
    public required RoomType RoomType { get; set; }
    [Required]
    [Range(1, 5, ErrorMessage = "Capacity must be between 1 and 5.")]
    public required int Capacity { get; set; } //kroz konstrukotr stavi default capacity
    [Required]
    [Range(1, 6, ErrorMessage = "Floor must be between 1 and 6.")]
    public required int Floor { get; set; }
    [Required]
    public bool IsAvailable { get; set; } = true;//na osnovu datuma mora da se proveri
    [Required]
    public required int PricePerNight { get; set; }//na osnovu tipa sobe i sprata
    public string? ImageUrl { get; set; }
    [JsonIgnore]
    public List<Reservation> Reservations { get; set; } = new List<Reservation>();
}
