using ParkShare.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace ParkShare.Application.DTOs.Rental;

public class UpdateRentalStatusDto
{
    [Required]
    public RentalStatus Status { get; set; }
}
