using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MyHotelApp.server.Models;

public class RoomService
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int RoomServiceID { get; set; }

    [Required]
    [StringLength(50, ErrorMessage = "Item name cannot exceed 50 characters.")]
    public required string ItemName { get; set; }

    [Required]
    public required decimal ItemPrice { get; set; }
    public string? Description { get; set; }
    public List<Reservation> AddedToReservations { get; set; } = new List<Reservation>();
}