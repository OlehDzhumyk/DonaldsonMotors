using DonaldsonMotors.API.DTOs.User;
using DonaldsonMotors.API.DTOs.Vehicle;
using DonaldsonMotors.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID claim not found or invalid.");
        }
        return userId;
    }


    [HttpGet("me")]
    [Authorize] 
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();

        try
        {
            _logger.LogInformation("Fetching profile for user ID {UserId}", userId);

            var profileDto = await _userService.GetProfileAsync(userId);

            _logger.LogInformation("Profile fetched successfully for user ID {UserId}", userId);
            return Ok(profileDto);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Profile not found for user ID {UserId}", userId);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching profile for user ID {UserId}", userId);
            return StatusCode(500, "An unexpected error occurred while fetching your profile.");
        }
    }

   

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto dto)
    {
        var userId = GetUserId();
        try
        {
            _logger.LogInformation("Updating profile for user ID {UserId}", userId);
            var updated = await _userService.UpdateProfileAsync(userId, dto);
            _logger.LogInformation("Profile updated successfully for user ID {UserId}", userId);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Update failed for user ID {UserId}", userId);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user ID {UserId}", userId);
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpPost("me/cars")]
    public async Task<IActionResult> AddVehicle([FromBody] VehicleRequestDto dto)
    {
        var userId = GetUserId();
        try
        {
            _logger.LogInformation("Adding vehicle for user ID {UserId}", userId);
            var created = await _userService.AddVehicleAsync(userId, dto);
            _logger.LogInformation("Vehicle added for user ID {UserId}", userId);
            return CreatedAtAction(nameof(GetProfile), new { }, created);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Vehicle with same registration already exists for user ID {UserId}", userId);
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding vehicle for user ID {UserId}", userId);
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpPut("me/cars/{registration}")]
    public async Task<IActionResult> UpdateVehicle(string registration, [FromBody] VehicleRequestDto dto)
    {
        var userId = GetUserId();
        try
        {
            _logger.LogInformation("Updating vehicle {Registration} for user ID {UserId}", registration, userId);
            await _userService.UpdateVehicleAsync(userId, registration, dto);
            _logger.LogInformation("Vehicle updated for user ID {UserId}", userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Vehicle update failed for user ID {UserId}", userId);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vehicle for user ID {UserId}", userId);
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpDelete("me/cars/{registration}")]
    public async Task<IActionResult> DeleteVehicle(string registration)
    {
        var userId = GetUserId();
        try
        {
            _logger.LogInformation("Deleting vehicle {Registration} for user ID {UserId}", registration, userId);
            await _userService.DeleteVehicleAsync(userId, registration);
            _logger.LogInformation("Vehicle deleted for user ID {UserId}", userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Vehicle delete failed for user ID {UserId}", userId);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting vehicle for user ID {UserId}", userId);
            return StatusCode(500, "An unexpected error occurred.");
        }
    }
}
