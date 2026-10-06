using DonaldsonMotors.API.DTOs.User;
using DonaldsonMotors.API.DTOs.Vehicle;
using DonaldsonMotors.API.Interfaces;
using DonaldsonMotors.API.Exceptions;
using DonaldsonMotors.API.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DonaldsonMotors.API.Controllers
{
    [ApiController]
    [Authorize] // All actions in this controller require authentication by default.
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

        /// <summary>
        /// Gets the current user's ID from the JWT token claims.
        /// </summary>
        /// <returns>The authenticated user's integer ID.</returns>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown if the user ID claim is missing from the token or is in an invalid format.
        /// </exception>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User ID claim not found or invalid.");
            }
            return userId;
        }

        /// <summary>
        /// Gets the profile of the currently authenticated user.
        /// </summary>
        /// <returns>The user's profile information.</returns>
        /// <response code="200">Returns the user's profile.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="404">If the user's profile is not found.</response>
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserProfileResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("Fetching profile for user ID {UserId}", userId);
                var profileDto = await _userService.GetProfileAsync(userId);
                _logger.LogInformation("Profile fetched successfully for user ID {UserId}", userId);
                return Ok(profileDto);
            }
            catch (UserProfileNotFoundException ex)
            {
                _logger.LogWarning(ex, "Profile not found for current user. Message: {ErrorMessage}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access in GetProfile. Message: {ErrorMessage}", ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user profile for current user.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while fetching your profile.");
            }
        }

        /// <summary>
        /// Updates the profile of the currently authenticated user.
        /// </summary>
        /// <param name="dto">The new profile data.</param>
        /// <returns>The updated user profile.</returns>
        /// <response code="200">Returns the updated profile.</response>
        /// <response code="400">If the update fails due to validation errors.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="404">If the user's profile is not found.</response>
        [HttpPut("me")]
        [ProducesResponseType(typeof(UserProfileResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("Updating profile for user ID {UserId}", userId);
                var updatedProfile = await _userService.UpdateProfileAsync(userId, dto);
                _logger.LogInformation("Profile updated successfully for user ID {UserId}", userId);
                return Ok(updatedProfile);
            }
            catch (UserProfileNotFoundException ex)
            {
                _logger.LogWarning(ex, "Update failed, profile not found. Message: {ErrorMessage}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (ProfileUpdateFailedException ex)
            {
                _logger.LogWarning(ex, "Profile update failed. Errors: {Errors}", string.Join(", ", ex.Errors));
                return BadRequest(new { title = "Profile update failed.", errors = ex.Errors });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt in UpdateProfile. Message: {ErrorMessage}", ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Allows the authenticated user to change their password.
        /// </summary>
        /// <param name="dto">The request containing the current and new passwords.</param>
        /// <response code="204">If the password was changed successfully.</response>
        /// <response code="400">If the password change fails (e.g., current password is wrong, new password is weak).</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="404">If the user's profile is not found.</response>
        [HttpPost("me/change-password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("User {UserId} attempting to change password.", userId);
                await _userService.ChangePasswordAsync(userId, dto);
                _logger.LogInformation("Password changed successfully for User {UserId}", userId);
                return NoContent();
            }
            catch (UserProfileNotFoundException ex)
            {
                // This should be rare if GetCurrentUserId works, but is included for completeness.
                _logger.LogWarning(ex, "Change password failed, user not found. Message: {ErrorMessage}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (ProfileUpdateFailedException ex) // Catches password change failures from the service layer.
            {
                _logger.LogWarning(ex, "Password change failed for user. Errors: {Errors}", string.Join(", ", ex.Errors));
                return BadRequest(new { title = "Password change failed.", errors = ex.Errors });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt in ChangePassword. Message: {ErrorMessage}", ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        // === Vehicle Management ===

        /// <summary>
        /// Adds a new vehicle to the authenticated user's profile.
        /// </summary>
        /// <param name="dto">The details of the vehicle to add.</param>
        /// <returns>The newly created vehicle.</returns>
        /// <response code="201">Returns the newly created vehicle.</response>
        /// <response code="400">If the request payload is invalid.</response>
        /// <response code="404">If the user's profile does not exist.</response>
        /// <response code="409">If a vehicle with the same registration already exists for this user.</response>
        [HttpPost("me/vehicles")]
        [ProducesResponseType(typeof(VehicleResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AddVehicle([FromBody] VehicleRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("Adding vehicle for user ID {UserId}", userId);
                var createdVehicle = await _userService.AddVehicleAsync(userId, dto);
                _logger.LogInformation("Vehicle added for user ID {UserId}", userId);
                // Points to GetProfile, which is a good way to confirm the vehicle list has been updated.
                return CreatedAtAction(nameof(GetProfile), null, createdVehicle);
            }
            catch (VehicleAlreadyExistsException ex)
            {
                _logger.LogWarning(ex, "Attempt to add vehicle that already exists. Message: {ErrorMessage}", ex.Message);
                return Conflict(ex.Message);
            }
            catch (UserProfileNotFoundException ex)
            {
                _logger.LogWarning(ex, "Cannot add vehicle, owner profile not found. Message: {ErrorMessage}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt in AddVehicle. Message: {ErrorMessage}", ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding vehicle for user.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Updates an existing vehicle for the authenticated user.
        /// </summary>
        /// <param name="registration">The registration number of the vehicle to update.</param>
        /// <param name="dto">The new details for the vehicle.</param>
        /// <response code="204">If the vehicle was updated successfully.</response>
        /// <response code="403">If the user does not own this vehicle.</response>
        /// <response code="404">If the vehicle with the given registration is not found.</response>
        [HttpPut("me/vehicles/{registration}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateVehicle(string registration, [FromBody] VehicleRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("Updating vehicle {Registration} for user ID {UserId}", registration, userId);
                await _userService.UpdateVehicleAsync(userId, registration, dto);
                _logger.LogInformation("Vehicle {Registration} updated for user ID {UserId}", registration, userId);
                return NoContent();
            }
            catch (VehicleNotFoundException ex)
            {
                _logger.LogWarning(ex, "Vehicle update failed, vehicle not found. Message: {ErrorMessage}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (VehicleAccessDeniedException ex)
            {
                _logger.LogWarning(ex, "Vehicle update failed, access denied. Message: {ErrorMessage}", ex.Message);
                return Forbid(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt in UpdateVehicle. Message: {ErrorMessage}", ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle for user.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Deletes a vehicle for the authenticated user.
        /// </summary>
        /// <param name="registration">The registration number of the vehicle to delete.</param>
        /// <response code="204">If the vehicle was deleted successfully.</response>
        /// <response code="400">If the vehicle cannot be deleted because it has active bookings.</response>
        /// <response code="403">If the user does not own this vehicle.</response>
        /// <response code="404">If the vehicle with the given registration is not found.</response>
        [HttpDelete("me/vehicles/{registration}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteVehicle(string registration)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("Deleting vehicle {Registration} for user ID {UserId}", registration, userId);
                await _userService.DeleteVehicleAsync(userId, registration);
                _logger.LogInformation("Vehicle {Registration} deleted for user ID {UserId}", registration, userId);
                return NoContent();
            }
            catch (VehicleNotFoundException ex)
            {
                _logger.LogWarning(ex, "Vehicle delete failed, vehicle not found. Message: {ErrorMessage}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (VehicleAccessDeniedException ex)
            {
                _logger.LogWarning(ex, "Vehicle delete failed, access denied. Message: {ErrorMessage}", ex.Message);
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex) // This is thrown by the service if the vehicle has active bookings.
            {
                _logger.LogWarning(ex, "Vehicle delete failed. Message: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt in DeleteVehicle. Message: {ErrorMessage}", ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle for user.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        // === ADMIN ACTIONS ===

        /// <summary>
        /// Updates a user's profile information. Accessible only by Managers.
        /// </summary>
        /// <param name="userIdToEdit">The ID of the user whose profile will be updated.</param>
        /// <param name="dto">The data to update.</param>
        /// <returns>The updated user profile.</returns>
        /// <response code="200">Returns the updated user profile.</response>
        /// <response code="400">If the update fails due to validation errors (e.g., invalid role).</response>
        /// <response code="404">If the user profile is not found.</response>
        /// <response code="409">If the new email address conflicts with an existing user.</response>
        [HttpPut("{userIdToEdit}")]
        [Authorize(Roles = Roles.Manager)]
        [ProducesResponseType(typeof(UserProfileResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AdminUpdateUser(int userIdToEdit, [FromBody] AdminUpdateUserRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation("Admin attempting to update profile for UserId {UserIdToEdit}", userIdToEdit);
                var updatedProfile = await _userService.AdminUpdateUserAsync(userIdToEdit, dto);
                _logger.LogInformation("Admin successfully updated profile for UserId {UserIdToEdit}", userIdToEdit);
                return Ok(updatedProfile);
            }
            catch (UserProfileNotFoundException ex)
            {
                _logger.LogWarning(ex, "Admin update failed, user profile not found. Message: {ErrorMessage}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (UserAlreadyExistsException ex)
            {
                _logger.LogWarning(ex, "Admin update failed, email conflict. Message: {ErrorMessage}", ex.Message);
                return Conflict(ex.Message);
            }
            catch (ProfileUpdateFailedException ex)
            {
                _logger.LogWarning(ex, "Admin update failed. Errors: {Errors}", string.Join(", ", ex.Errors));
                return BadRequest(new { title = "Profile update by admin failed.", errors = ex.Errors });
            }
            catch (InvalidRoleException ex)
            {
                _logger.LogWarning(ex, "Admin update failed, invalid role specified. Message: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin update for UserId {UserIdToEdit}", userIdToEdit);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }
    }
}