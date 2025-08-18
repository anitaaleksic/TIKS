using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MyHotelApp.server.Models;


public class RoomType
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int RoomTypeID { get; set; }
    [Required]
    public required string Type { get; set; }
    [Required]
    [Range(1, 5, ErrorMessage = "Capacity must be between 1 and 5.")]
    public required int Capacity { get; set; } //kroz konstrukotr stavi default capacity
    [Required]
    public required decimal PricePerNight { get; set; }//na osnovu tipa sobe
    [JsonIgnore]
    public List<Room> Rooms { get; set; } = new List<Room>();
}

//(pricePerNight + ExtraServices) * NumberOfNights + RoomServices

public class RoomTypeDTO
{
    public int RoomTypeID { get; set; }
    public string Type { get; set; }
    public int Capacity { get; set; }
    public decimal PricePerNight { get; set; }
}

