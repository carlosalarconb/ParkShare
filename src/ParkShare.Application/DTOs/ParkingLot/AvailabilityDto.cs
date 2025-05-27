using System;
using System.ComponentModel.DataAnnotations;

namespace ParkShare.Application.DTOs.ParkingLot;

public class AvailabilityDto
{
    public Guid Id { get; set; } // Useful if an availability slot can be individually identified/modified
    [Required]
    public DayOfWeek DayOfWeek { get; set; }
    [Required]
    public TimeSpan StartTime { get; set; } // "HH:mm:ss"
    [Required]
    public TimeSpan EndTime { get; set; }   // "HH:mm:ss"
    public bool IsAvailable { get; set; } = true;
}
