using ParkShare.Application.DTOs.ParkingLot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParkShare.Application.Interfaces;

public interface IParkingLotService
{
    Task<ParkingLotDto> CreateParkingLotAsync(CreateParkingLotDto createDto, string ownerId);
    Task<ParkingLotDto?> GetParkingLotByIdAsync(Guid id);
    Task<IEnumerable<ParkingLotDto>> GetAllParkingLotsAsync();
    Task<IEnumerable<ParkingLotDto>> GetParkingLotsByOwnerAsync(string ownerId);
    Task<bool> UpdateParkingLotAsync(Guid id, UpdateParkingLotDto updateDto, string ownerId);
    Task<bool> DeleteParkingLotAsync(Guid id, string ownerId);
    // Add methods for managing availability if handled by this service
    Task<bool> AddAvailabilityAsync(Guid parkingLotId, AvailabilityDto availabilityDto, string ownerId);
    Task<IEnumerable<AvailabilityDto>> GetAvailabilitiesAsync(Guid parkingLotId);
}
