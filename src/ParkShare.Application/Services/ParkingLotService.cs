using Microsoft.EntityFrameworkCore;
using ParkShare.Application.DTOs.ParkingLot;
using ParkShare.Application.Interfaces;
using ParkShare.Core.Entities;
using ParkShare.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkShare.Application.Services;

public class ParkingLotService : IParkingLotService
{
    private readonly ParkShareDbContext _dbContext;

    public ParkingLotService(ParkShareDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ParkingLotDto> CreateParkingLotAsync(CreateParkingLotDto createDto, string ownerId)
    {
        if (!Guid.TryParse(ownerId, out Guid ownerGuid))
        {
            throw new ArgumentException("Invalid owner ID format.", nameof(ownerId));
        }

        var parkingLot = new ParkingLot
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerGuid, // Use parsed Guid
            Address = createDto.Address,
            City = createDto.City,
            Country = createDto.Country,
            PostalCode = createDto.PostalCode,
            Latitude = createDto.Latitude,
            Longitude = createDto.Longitude,
            HourlyRate = createDto.HourlyRate,
            Description = createDto.Description,
            IsActive = true, // Default to active
            CreatedAtUtc = DateTime.UtcNow,
            Availabilities = new List<Availability>() // Initialize empty list
        };

        _dbContext.ParkingLots.Add(parkingLot);
        await _dbContext.SaveChangesAsync();

        return MapParkingLotToDto(parkingLot);
    }

    public async Task<ParkingLotDto?> GetParkingLotByIdAsync(Guid id)
    {
        var parkingLot = await _dbContext.ParkingLots
                                         .Include(p => p.Availabilities)
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(p => p.Id == id);

        return parkingLot == null ? null : MapParkingLotToDto(parkingLot);
    }

    public async Task<IEnumerable<ParkingLotDto>> GetAllParkingLotsAsync()
    {
        return await _dbContext.ParkingLots
                               .Include(p => p.Availabilities)
                               .AsNoTracking()
                               .Select(p => MapParkingLotToDto(p))
                               .ToListAsync();
    }

    public async Task<IEnumerable<ParkingLotDto>> GetParkingLotsByOwnerAsync(string ownerId)
    {
        if (!Guid.TryParse(ownerId, out Guid ownerGuid))
        {
            // Or return empty list, or throw specific exception
            return Enumerable.Empty<ParkingLotDto>();
        }
        return await _dbContext.ParkingLots
                               .Where(p => p.OwnerId == ownerGuid) // Use parsed Guid
                               .Include(p => p.Availabilities)
                               .AsNoTracking()
                               .Select(p => MapParkingLotToDto(p))
                               .ToListAsync();
    }

    public async Task<bool> UpdateParkingLotAsync(Guid id, UpdateParkingLotDto updateDto, string ownerId)
    {
        if (!Guid.TryParse(ownerId, out Guid ownerGuid))
        {
            return false; // Or throw ArgumentException
        }
        var parkingLot = await _dbContext.ParkingLots
                                         .Include(p => p.Availabilities) 
                                         .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == ownerGuid); // Use parsed Guid

        if (parkingLot == null) return false;

        if (updateDto.Address != null) parkingLot.Address = updateDto.Address;
        if (updateDto.City != null) parkingLot.City = updateDto.City;
        if (updateDto.Country != null) parkingLot.Country = updateDto.Country;
        if (updateDto.PostalCode != null) parkingLot.PostalCode = updateDto.PostalCode;
        if (updateDto.Latitude.HasValue) parkingLot.Latitude = updateDto.Latitude.Value;
        if (updateDto.Longitude.HasValue) parkingLot.Longitude = updateDto.Longitude.Value;
        if (updateDto.HourlyRate.HasValue) parkingLot.HourlyRate = updateDto.HourlyRate.Value;
        if (updateDto.Description != null) parkingLot.Description = updateDto.Description;
        if (updateDto.IsActive.HasValue) parkingLot.IsActive = updateDto.IsActive.Value;
        
        parkingLot.UpdatedAtUtc = DateTime.UtcNow;

        _dbContext.ParkingLots.Update(parkingLot);
        return await _dbContext.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteParkingLotAsync(Guid id, string ownerId)
    {
        if (!Guid.TryParse(ownerId, out Guid ownerGuid))
        {
            return false; // Or throw ArgumentException
        }
        var parkingLot = await _dbContext.ParkingLots.FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == ownerGuid); // Use parsed Guid
        if (parkingLot == null) return false;

        _dbContext.ParkingLots.Remove(parkingLot);
        return await _dbContext.SaveChangesAsync() > 0;
    }

    public async Task<bool> AddAvailabilityAsync(Guid parkingLotId, AvailabilityDto availabilityDto, string ownerId)
    {
        if (!Guid.TryParse(ownerId, out Guid ownerGuid))
        {
            return false; // Or throw ArgumentException
        }
        var parkingLot = await _dbContext.ParkingLots.FirstOrDefaultAsync(p => p.Id == parkingLotId && p.OwnerId == ownerGuid); // Use parsed Guid
        if (parkingLot == null) return false; 

        var availability = new Availability
        {
            Id = Guid.NewGuid(),
            ParkingLotId = parkingLotId,
            DayOfWeek = availabilityDto.DayOfWeek,
            StartTime = availabilityDto.StartTime,
            EndTime = availabilityDto.EndTime,
            IsAvailable = availabilityDto.IsAvailable
        };
        
        _dbContext.Availabilities.Add(availability);
        return await _dbContext.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<AvailabilityDto>> GetAvailabilitiesAsync(Guid parkingLotId)
    {
        return await _dbContext.Availabilities
                               .Where(a => a.ParkingLotId == parkingLotId)
                               .AsNoTracking()
                               .Select(a => MapAvailabilityToDto(a))
                               .ToListAsync();
    }

    private static ParkingLotDto MapParkingLotToDto(ParkingLot parkingLot)
    {
        return new ParkingLotDto
        {
            Id = parkingLot.Id,
            OwnerId = parkingLot.OwnerId.ToString(), // Convert Guid to string for DTO
            Address = parkingLot.Address,
            City = parkingLot.City,
            Country = parkingLot.Country,
            PostalCode = parkingLot.PostalCode,
            Latitude = parkingLot.Latitude,
            Longitude = parkingLot.Longitude,
            HourlyRate = parkingLot.HourlyRate,
            Description = parkingLot.Description,
            IsActive = parkingLot.IsActive,
            Availabilities = parkingLot.Availabilities?.Select(MapAvailabilityToDto).ToList() ?? new List<AvailabilityDto>()
        };
    }

    private static AvailabilityDto MapAvailabilityToDto(Availability availability)
    {
        return new AvailabilityDto
        {
            Id = availability.Id,
            DayOfWeek = availability.DayOfWeek,
            StartTime = availability.StartTime,
            EndTime = availability.EndTime,
            IsAvailable = availability.IsAvailable
        };
    }
}
