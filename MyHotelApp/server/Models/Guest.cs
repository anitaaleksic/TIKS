using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MyHotelApp.server.Models;

public class Guest
{
    [Key]
    [StringLength(13, ErrorMessage = "JMBG must be 13 characters long.")]
    public required string JMBG { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
    public required string FullName { get; set; }

    [Required]
    [Phone(ErrorMessage = "Invalid phone number.")]
    [StringLength(13, MinimumLength = 12, ErrorMessage = "Phone number must be between 12 and 13 characters long.")]
    public required string PhoneNumber { get; set; }
    [JsonIgnore]
    public List<Reservation> Reservations { get; set; } = new List<Reservation>();

}