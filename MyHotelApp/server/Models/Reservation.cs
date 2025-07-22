using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MyHotelApp.server.Models;

public class Reservation
{
    [Key]
    public int ReservationID { get; set; }
    [Required]
    public int RoomNumber { get; set; }
    [ForeignKey("RoomNumber")]
    public Room Room { get; set; }
    [Required]
    public string GuestID { get; set; }
    [ForeignKey("GuestID")]
    public Guest Guest { get; set; }

    // public int EmployeeID { get; set; }
    // public Employee Employee { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime CheckInDate { get; set; }
    [Required]
    [DataType(DataType.Date)]
    public DateTime CheckOutDate { get; set; }
    [Required]
    public decimal TotalPrice { get; set; }
    public List<RoomService> RoomServices { get; set; }
    public List<ExtraService> ExtraServices { get; set; }
}
