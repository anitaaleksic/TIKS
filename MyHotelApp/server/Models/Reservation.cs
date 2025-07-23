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

    // public int EmployeeID { get; set; }
    // public Employee Employee { get; set; }

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
