using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MyHotelApp.server.Models;

public class Reservation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ReservationID { get; set; }
    [Required]
    public required int RoomNumber { get; set; }
    [ForeignKey("RoomNumber")]
    public Room Room { get; set; }
    [Required]
    public required string GuestID { get; set; }
    [ForeignKey("GuestID")]
    public Guest Guest { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public required DateTime CheckInDate { get; set; }
    [Required]
    [DataType(DataType.Date)]
    public required DateTime CheckOutDate { get; set; }
    [Required]
    public decimal TotalPrice { get; set; }//obracunava se
    [JsonIgnore]
    public List<RoomService> RoomServices { get; set; } = new List<RoomService>();
    [JsonIgnore]
    public List<ExtraService> ExtraServices { get; set; } = new List<ExtraService>();
}

public class ReservationDTO
{
    public int ReservationID { get; set; }
    public int RoomNumber { get; set; }
    public string GuestID { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public decimal TotalPrice { get; set; }
    public List<int> RoomServiceIDs { get; set; } = new();
    public List<int> ExtraServiceIDs { get; set; } = new();
}
