using Microsoft.EntityFrameworkCore;
using ParkShare.Application.DTOs.Rental;
using ParkShare.Application.Interfaces;
using ParkShare.Core.Entities;
using ParkShare.Core.Enums;
using ParkShare.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkShare.Application.Services;

public class RentalService : IRentalService
{
    private readonly ParkShareDbContext _dbContext;

    public RentalService(ParkShareDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RentalDto> CreateRentalAsync(CreateRentalDto createDto, string renterId)
    {
        if (!Guid.TryParse(renterId, out Guid renterGuid))
        {
            throw new ArgumentException("Invalid renter ID format.", nameof(renterId));
        }

        var parkingLot = await _dbContext.ParkingLots.FindAsync(createDto.ParkingLotId);
        if (parkingLot == null || !parkingLot.IsActive)
        {
            throw new InvalidOperationException("Parking lot is not available or does not exist.");
        }

        if (createDto.StartTimeUtc >= createDto.EndTimeUtc || createDto.StartTimeUtc < DateTime.UtcNow)
        {
            throw new ArgumentException("Invalid rental time range.");
        }

        // TODO: Implement availability conflict checking
        // This would involve checking parkingLot.Availabilities and existing Rentals.
        // For example:
        // bool isSlotAvailable = parkingLot.Availabilities
        //    .Any(a => a.DayOfWeek == createDto.StartTimeUtc.DayOfWeek &&
        //                a.StartTime <= createDto.StartTimeUtc.TimeOfDay &&
        //                a.EndTime >= createDto.EndTimeUtc.TimeOfDay &&
        //                a.IsAvailable);
        // if (!isSlotAvailable) throw new InvalidOperationException("Parking lot is not available for the selected time slot.");
        //
        // var conflictingRental = await _dbContext.Rentals
        //    .AnyAsync(r => r.ParkingLotId == createDto.ParkingLotId &&
        //                   r.Status != RentalStatus.Cancelled &&
        //                   r.Status != RentalStatus.Completed &&
        //                   (createDto.StartTimeUtc < r.EndTimeUtc && createDto.EndTimeUtc > r.StartTimeUtc));
        // if (conflictingRental) throw new InvalidOperationException("Selected time slot is already booked.");


        var duration = (createDto.EndTimeUtc - createDto.StartTimeUtc).TotalHours;
        var totalCost = (decimal)duration * parkingLot.HourlyRate;

        var rental = new Rental
        {
            Id = Guid.NewGuid(),
            ParkingLotId = createDto.ParkingLotId,
            RenterId = renterGuid, // Use parsed Guid
            StartTimeUtc = createDto.StartTimeUtc,
            EndTimeUtc = createDto.EndTimeUtc,
            TotalCost = totalCost,
            Status = RentalStatus.Booked,
            BookedAtUtc = DateTime.UtcNow
        };

        _dbContext.Rentals.Add(rental);
        await _dbContext.SaveChangesAsync();

        return MapRentalToDto(rental);
    }

    public async Task<RentalDto?> GetRentalByIdAsync(Guid id)
    {
        var rental = await _dbContext.Rentals.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        return rental == null ? null : MapRentalToDto(rental);
    }

    public async Task<IEnumerable<RentalDto>> GetRentalsByRenterAsync(string renterId)
    {
        if (!Guid.TryParse(renterId, out Guid renterGuid))
        {
            return Enumerable.Empty<RentalDto>(); // Or throw
        }
        return await _dbContext.Rentals
                               .Where(r => r.RenterId == renterGuid) // Use parsed Guid
                               .AsNoTracking()
                               .Select(r => MapRentalToDto(r))
                               .ToListAsync();
    }

    public async Task<IEnumerable<RentalDto>> GetRentalsByParkingLotAsync(Guid parkingLotId)
    {
        return await _dbContext.Rentals
                               .Where(r => r.ParkingLotId == parkingLotId)
                               .AsNoTracking()
                               .Select(r => MapRentalToDto(r))
                               .ToListAsync();
    }

    public async Task<bool> UpdateRentalStatusAsync(Guid id, UpdateRentalStatusDto statusDto)
    {
        var rental = await _dbContext.Rentals.FirstOrDefaultAsync(r => r.Id == id); // Track entity for update
        if (rental == null) return false;

        // Basic validation for status transitions
        if (statusDto.Status == RentalStatus.Cancelled)
        {
            if (rental.Status == RentalStatus.Active || rental.Status == RentalStatus.Completed)
            {
                return false; // Cannot cancel an already active or completed rental
            }
            // Potentially add time-based cancellation policies (e.g., cannot cancel 1 hour before start)
        }

        rental.Status = statusDto.Status;
        // Consider adding an UpdatedAtUtc field to Rental entity and set it here
        // rental.UpdatedAtUtc = DateTime.UtcNow;

        _dbContext.Rentals.Update(rental);
        return await _dbContext.SaveChangesAsync() > 0;
    }

    private static RentalDto MapRentalToDto(Rental rental)
    {
        return new RentalDto
        {
            Id = rental.Id,
            ParkingLotId = rental.ParkingLotId,
            RenterId = rental.RenterId.ToString(), // Convert Guid to string for DTO
            StartTimeUtc = rental.StartTimeUtc,
            EndTimeUtc = rental.EndTimeUtc,
            TotalCost = rental.TotalCost,
            Status = rental.Status,
            BookedAtUtc = rental.BookedAtUtc
        };
    }
}
