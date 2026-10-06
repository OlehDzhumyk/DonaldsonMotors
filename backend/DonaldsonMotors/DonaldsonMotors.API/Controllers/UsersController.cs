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
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Fetching profile for user ID {UserId}", userId);
            var profileDto = await _userService.GetProfileAsync(userId);
            _logger.LogInformation("Profile fetched successfully for user ID {UserId}", userId);
            return Ok(profileDto);
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
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto dto)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Updating profile for user ID {UserId}", userId);
            var updatedProfile = await _userService.UpdateProfileAsync(userId, dto);
            _logger.LogInformation("Profile updated successfully for user ID {UserId}", userId);
            return Ok(updatedProfile);
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
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto dto)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} attempting to change password.", userId);
            await _userService.ChangePasswordAsync(userId, dto);
            _logger.LogInformation("Password changed successfully for User {UserId}", userId);
            return NoContent();
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
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AddVehicle([FromBody] VehicleRequestDto dto)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Adding vehicle for user ID {UserId}", userId);
            var createdVehicle = await _userService.AddVehicleAsync(userId, dto);
            _logger.LogInformation("Vehicle added for user ID {UserId}", userId);
            // Points to GetProfile, which is a good way to confirm the vehicle list has been updated.
            return CreatedAtAction(nameof(GetProfile), null, createdVehicle);
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
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateVehicle(string registration, [FromBody] VehicleRequestDto dto)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Updating vehicle {Registration} for user ID {UserId}", registration, userId);
            await _userService.UpdateVehicleAsync(userId, registration, dto);
            _logger.LogInformation("Vehicle {Registration} updated for user ID {UserId}", registration, userId);
            return NoContent();
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
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteVehicle(string registration)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Deleting vehicle {Registration} for user ID {UserId}", registration, userId);
            await _userService.DeleteVehicleAsync(userId, registration);
            _logger.LogInformation("Vehicle {Registration} deleted for user ID {UserId}", registration, userId);
            return NoContent();
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
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AdminUpdateUser(int userIdToEdit, [FromBody] AdminUpdateUserRequestDto dto)
        {
            _logger.LogInformation("Admin attempting to update profile for UserId {UserIdToEdit}", userIdToEdit);
            var updatedProfile = await _userService.AdminUpdateUserAsync(userIdToEdit, dto);
            _logger.LogInformation("Admin successfully updated profile for UserId {UserIdToEdit}", userIdToEdit);
            return Ok(updatedProfile);
        }
    }
}