using System;
using System.Collections.Generic; // For AvailabilityDto collection

namespace ParkShare.Application.DTOs.ParkingLot;

public class ParkingLotDto
{
    public Guid Id { get; set; }
    public string OwnerId { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string PostalCode { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public decimal HourlyRate { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public List<AvailabilityDto> Availabilities { get; set; } = new List<AvailabilityDto>();
}
