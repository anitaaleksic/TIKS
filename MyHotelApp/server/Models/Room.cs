using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MyHotelApp.server.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MyHotelApp.server.Models;

public class Room
{
    [Key]
    [Range(1, 699, ErrorMessage = "Room number must be between 1 and 699.")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required int RoomNumber { get; set; }
    [Required]
    public required int RoomTypeID { get; set; }
    [ForeignKey("RoomTypeID")]
    public RoomType RoomType { get; set; }
    [Required]
    [Range(1, 6, ErrorMessage = "Floor must be between 1 and 6.")]
    public required int Floor { get; set; }
    [Required]
    public bool IsAvailable { get; set; } = true;//na osnovu datuma mora da se proveri
    [JsonIgnore]
    public List<Reservation> Reservations { get; set; } = new List<Reservation>();
}

//(pricePerNight + ExtraServices) * NumberOfNights + RoomServices

public class RoomDTO
{
    public int RoomNumber { get; set; }
    public int RoomTypeID { get; set; }
    public int Floor { get; set; }
    public bool IsAvailable { get; set; }
}

