using System.ComponentModel.DataAnnotations; // For basic validation attributes

namespace ParkShare.Application.DTOs.ParkingLot;

public class CreateParkingLotDto
{
    [Required]
    public string Address { get; set; }
    [Required]
    public string City { get; set; }
    [Required]
    public string Country { get; set; }
    [Required]
    public string PostalCode { get; set; }
    [Range(-90, 90)]
    public double Latitude { get; set; }
    [Range(-180, 180)]
    public double Longitude { get; set; }
    [Range(0.01, 1000)] // Example range for hourly rate
    public decimal HourlyRate { get; set; }
    public string? Description { get; set; }
}
