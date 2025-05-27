using ParkShare.Application.DTOs.Rental;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParkShare.Application.Interfaces;

public interface IRentalService
{
    Task<RentalDto> CreateRentalAsync(CreateRentalDto createDto, string renterId);
    Task<RentalDto?> GetRentalByIdAsync(Guid id);
    Task<IEnumerable<RentalDto>> GetRentalsByRenterAsync(string renterId);
    Task<IEnumerable<RentalDto>> GetRentalsByParkingLotAsync(Guid parkingLotId);
    Task<bool> UpdateRentalStatusAsync(Guid id, UpdateRentalStatusDto statusDto); // e.g., for cancellation
}
