using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MyHotelApp.server.Models;

public class Guest
{
    [Key]
    [StringLength(13, ErrorMessage = "JMBG must be 13 characters long.")]
    public string JMBG { get; set; }
    [Required]
    [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
    public string FullName { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; }

    [Phone(ErrorMessage = "Invalid phone number.")]
    public string PhoneNumber { get; set; }

    public List<Reservation> Reservations { get; set; }

}