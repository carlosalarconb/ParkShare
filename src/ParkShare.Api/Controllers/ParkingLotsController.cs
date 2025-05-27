using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkShare.Application.DTOs.ParkingLot;
using ParkShare.Application.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ParkShare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ParkingLotsController : ControllerBase
{
    private readonly IParkingLotService _parkingLotService;

    public ParkingLotsController(IParkingLotService parkingLotService)
    {
        _parkingLotService = parkingLotService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("User ID not found in token.");


    [HttpPost]
    [Authorize] // Requires authentication
    public async Task<IActionResult> CreateParkingLot([FromBody] CreateParkingLotDto createDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var ownerId = GetUserId();
        var parkingLot = await _parkingLotService.CreateParkingLotAsync(createDto, ownerId);
        return CreatedAtAction(nameof(GetParkingLotById), new { id = parkingLot.Id }, parkingLot);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetParkingLotById(Guid id)
    {
        var parkingLot = await _parkingLotService.GetParkingLotByIdAsync(id);
        if (parkingLot == null) return NotFound();
        return Ok(parkingLot);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllParkingLots()
    {
        var parkingLots = await _parkingLotService.GetAllParkingLotsAsync();
        return Ok(parkingLots);
    }

    [HttpGet("my-lots")]
    [Authorize] // Requires authentication
    public async Task<IActionResult> GetMyParkingLots()
    {
        var ownerId = GetUserId();
        var parkingLots = await _parkingLotService.GetParkingLotsByOwnerAsync(ownerId);
        return Ok(parkingLots);
    }

    [HttpPut("{id:guid}")]
    [Authorize] // Requires authentication
    public async Task<IActionResult> UpdateParkingLot(Guid id, [FromBody] UpdateParkingLotDto updateDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var ownerId = GetUserId();
        var success = await _parkingLotService.UpdateParkingLotAsync(id, updateDto, ownerId);
        if (!success) return NotFound("Parking lot not found or user not authorized to update."); // Or Forbidden
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize] // Requires authentication
    public async Task<IActionResult> DeleteParkingLot(Guid id)
    {
        var ownerId = GetUserId();
        var success = await _parkingLotService.DeleteParkingLotAsync(id, ownerId);
        if (!success) return NotFound("Parking lot not found or user not authorized to delete."); // Or Forbidden
        return NoContent();
    }

    // --- Availability Endpoints ---
    [HttpPost("{parkingLotId:guid}/availability")]
    [Authorize] // Requires authentication
    public async Task<IActionResult> AddAvailability(Guid parkingLotId, [FromBody] AvailabilityDto availabilityDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var ownerId = GetUserId(); 
        // Ensure the caller owns the parkingLotId - service method should check this
        var success = await _parkingLotService.AddAvailabilityAsync(parkingLotId, availabilityDto, ownerId);
        if (!success) return Conflict("Could not add availability. Ensure parking lot exists and you are the owner.");
        // Consider returning the created availability or a link to it.
        return Ok("Availability added."); 
    }

    [HttpGet("{parkingLotId:guid}/availability")]
    public async Task<IActionResult> GetAvailabilities(Guid parkingLotId)
    {
        var availabilities = await _parkingLotService.GetAvailabilitiesAsync(parkingLotId);
        return Ok(availabilities);
    }
}
