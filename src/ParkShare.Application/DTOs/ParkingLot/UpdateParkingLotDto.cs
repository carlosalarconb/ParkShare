using System.ComponentModel.DataAnnotations;

namespace ParkShare.Application.DTOs.ParkingLot;

public class UpdateParkingLotDto
{
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    [Range(-90, 90)]
    public double? Latitude { get; set; }
    [Range(-180, 180)]
    public double? Longitude { get; set; }
    [Range(0.01, 1000)]
    public decimal? HourlyRate { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}
