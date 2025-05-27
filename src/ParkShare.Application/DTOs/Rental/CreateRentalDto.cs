using System;
using System.ComponentModel.DataAnnotations;

namespace ParkShare.Application.DTOs.Rental;

public class CreateRentalDto
{
    [Required]
    public Guid ParkingLotId { get; set; }
    [Required]
    public DateTime StartTimeUtc { get; set; }
    [Required]
    public DateTime EndTimeUtc { get; set; }
    // TotalCost will be calculated by the backend
}
