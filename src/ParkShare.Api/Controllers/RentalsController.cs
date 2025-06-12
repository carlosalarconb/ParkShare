using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkShare.Application.DTOs.Rental;
using ParkShare.Application.Interfaces;
using ParkShare.Core.Enums; // For RentalStatus
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ParkShare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Most rental actions require authentication
public class RentalsController : ControllerBase
{
    private readonly IRentalService _rentalService;

    public RentalsController(IRentalService rentalService)
    {
        _rentalService = rentalService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("User ID not found in token.");

    [HttpPost]
    public async Task<IActionResult> CreateRental([FromBody] CreateRentalDto createDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var renterId = GetUserId();
        try
        {
            var rental = await _rentalService.CreateRentalAsync(createDto, renterId);
            return CreatedAtAction(nameof(GetRentalById), new { id = rental.Id }, rental);
        }
        catch (InvalidOperationException ex) // e.g., parking lot not available
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex) // e.g., invalid time range
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRentalById(Guid id)
    {
        var rental = await _rentalService.GetRentalByIdAsync(id);
        if (rental == null) return NotFound();
        // Add authorization check: only renter or parking lot owner should see this?
        // For now, if authenticated and ID exists, show.
        return Ok(rental);
    }

    [HttpGet("my-rentals")]
    public async Task<IActionResult> GetMyRentals()
    {
        var renterId = GetUserId();
        var rentals = await _rentalService.GetRentalsByRenterAsync(renterId);
        return Ok(rentals);
    }

    [HttpGet("parking-lot/{parkingLotId:guid}")]
    public async Task<IActionResult> GetRentalsByParkingLot(Guid parkingLotId)
    {
        // Add authorization check: only parking lot owner should see this?
        var rentals = await _rentalService.GetRentalsByParkingLotAsync(parkingLotId);
        return Ok(rentals);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateRentalStatus(Guid id, [FromBody] UpdateRentalStatusDto statusDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Add authorization: Who can update status? Renter for cancellation? Owner for other changes?
        // For now, keeping it simple. Service layer has basic validation.
        var success = await _rentalService.UpdateRentalStatusAsync(id, statusDto);
        if (!success) return NotFound("Rental not found or status update failed.");
        return NoContent();
    }
}
