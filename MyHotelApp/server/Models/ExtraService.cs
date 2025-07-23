using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;


namespace MyHotelApp.server.Models;

public class ExtraService
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ExtraServiceID { get; set; }
    [Required]
    [StringLength(100, ErrorMessage = "Service name cannot exceed 100 characters.")]
    public required string ServiceName { get; set; }
    [Required]
    public decimal Price { get; set; }
    public string? Description { get; set; }
    //ZA PARKING BR PARKING MESTA NE MOZE DA IH BUDE REZ VISE NEGO STO IMA PARKING MESTA 
    //ISTO I ZA RESTORAN I ZA WELLNESS

    public List<Reservation> AddedToReservations { get; set; } = new List<Reservation>();
}
