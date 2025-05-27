using ParkShare.Core.Enums; // For RentalStatus
using System;

namespace ParkShare.Application.DTOs.Rental;

public class RentalDto
{
    public Guid Id { get; set; }
    public Guid ParkingLotId { get; set; }
    public string RenterId { get; set; }
    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }
    public decimal TotalCost { get; set; }
    public RentalStatus Status { get; set; }
    public DateTime BookedAtUtc { get; set; }
}
