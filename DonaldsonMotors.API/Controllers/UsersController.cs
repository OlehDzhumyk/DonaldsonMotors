using DonaldsonMotors.API.Domain.Models;
using DonaldsonMotors.API.DTOs.User;
using DonaldsonMotors.API.Interfaces.Services;
using DonaldsonMotors.API.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _svc;
    private readonly IVehicleRepository _repo;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService svc, IVehicleRepository vehicles, ILogger<UsersController> logger)
    {
        _svc = svc;
        _repo = vehicles;
        _logger = logger;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var id = GetUserId();
        _logger.LogInformation("Fetching profile for user ID {UserId}", id);

        var user = await _svc.GetProfileAsync(id);
        if (user is null)
        {
            _logger.LogWarning("User profile not found for ID {UserId}", id);
            return NotFound();
        }

        var vehicles = await _repo.GetByCustomerIdAsync(id);
        _logger.LogInformation("Returning profile and {VehicleCount} vehicles for user ID {UserId}", vehicles.Count(), id);

        return Ok(user.ToResponseDto(vehicles.Select(v => v.ToDomainModel())));
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto dto)
    {
        var id = GetUserId();
        _logger.LogInformation("User {UserId} attempting to update profile", id);

        var ok = await _svc.UpdateProfileAsync(id, dto.FullName, dto.Address, dto.TelephoneNumber);
        if (!ok)
        {
            _logger.LogWarning("Failed to update profile for user {UserId}", id);
            return NotFound();
        }

        _logger.LogInformation("Profile updated successfully for user {UserId}", id);
        return NoContent();
    }

    [HttpPost("me/cars")]
    public async Task<IActionResult> AddVehicle([FromBody] Vehicle vehicle)
    {
        var id = GetUserId();
        _logger.LogInformation("User {UserId} adding new vehicle with registration {Reg}", id, vehicle.Registration);

        var created = await _svc.AddVehicleAsync(id, vehicle);
        _logger.LogInformation("Vehicle {Reg} created successfully for user {UserId}", created.Registration, id);

        return CreatedAtAction(nameof(GetProfile), new { }, created.ToResponseDto());
    }

    [HttpPut("me/cars/{registration}")]
    public async Task<IActionResult> UpdateVehicle(string registration, [FromBody] Vehicle vehicle)
    {
        var id = GetUserId();
        _logger.LogInformation("User {UserId} attempting to update vehicle {Reg}", id, registration);

        var ok = await _svc.UpdateVehicleAsync(id, registration, vehicle);
        if (!ok)
        {
            _logger.LogWarning("Update failed: vehicle {Reg} not found or doesn't belong to user {UserId}", registration, id);
            return NotFound();
        }

        _logger.LogInformation("Vehicle {Reg} updated successfully for user {UserId}", registration, id);
        return NoContent();
    }

    [HttpDelete("me/cars/{registration}")]
    public async Task<IActionResult> DeleteVehicle(string registration)
    {
        var id = GetUserId();
        _logger.LogInformation("User {UserId} attempting to delete vehicle {Reg}", id, registration);

        var ok = await _svc.DeleteVehicleAsync(id, registration);
        if (!ok)
        {
            _logger.LogWarning("Delete failed: vehicle {Reg} not found or doesn't belong to user {UserId}", registration, id);
            return NotFound();
        }

        _logger.LogInformation("Vehicle {Reg} deleted successfully for user {UserId}", registration, id);
        return NoContent();
    }
}
