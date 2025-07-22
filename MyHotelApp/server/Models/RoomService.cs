using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MyHotelApp.server.Models;

public class RoomService
{
    [Key]
    public int ServiceID { get; set; }

    [Required]
    [StringLength(50, ErrorMessage = "Item name cannot exceed 50 characters.")]
    public string ItemName { get; set; }

    [Required]
    public decimal ItemPrice { get; set; }

    [Required]
    public string Description { get; set; }
    public List<Reservation> AddedToReservations { get; set; }
}