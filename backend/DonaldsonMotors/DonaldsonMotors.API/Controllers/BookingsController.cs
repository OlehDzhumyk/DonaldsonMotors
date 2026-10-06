using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DonaldsonMotors.API.DTOs.Booking;
using DonaldsonMotors.API.Interfaces;
using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Exceptions;
using System.Security.Claims;

namespace DonaldsonMotors.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All endpoints in this controller require authentication by default.
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(IBookingService bookingService, ILogger<BookingsController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        /// <summary>
        /// Gets the current user's ID from the JWT token claims.
        /// </summary>
        /// <returns>The authenticated user's integer ID.</returns>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown if the user ID claim is missing or invalid. This should not happen
        /// for endpoints protected by the [Authorize] attribute.
        /// </exception>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User ID claim not found or invalid in token.");
            }
            return userId;
        }

        // === CUSTOMER ENDPOINTS ===

        /// <summary>
        /// Creates a new booking. Accessible only by customers.
        /// </summary>
        /// <param name="dto">The details for the new booking.</param>
        /// <returns>The newly created booking details.</returns>
        /// <response code="201">Returns the newly created booking.</response>
        /// <response code="400">If the request has invalid data (e.g., invalid service type, bad request format).</response>
        /// <response code="403">If the customer tries to book for a vehicle they do not own.</response>
        /// <response code="404">If the customer's profile or specified vehicle is not found.</response>
        /// <response code="409">If the requested time slot is no longer available.</response>
        [HttpPost]
        [Authorize(Roles = Roles.Customer)]
        [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequestDto dto)
        {
            try
            {
                var customerId = GetCurrentUserId();
                var createdBooking = await _bookingService.CreateBookingAsync(customerId, dto);
                // Returns a 201 Created status with a location header pointing to the new resource.
                return CreatedAtAction(nameof(GetMyBookings), null, createdBooking);
            }
            catch (UserProfileNotFoundException ex) { return NotFound(ex.Message); }
            catch (VehicleAccessDeniedException ex) { return Forbid(ex.Message); }
            catch (ServiceTypeNotFoundException ex) { return BadRequest(ex.Message); }
            catch (SlotUnavailableException ex) { return Conflict(ex.Message); }
            catch (BookingOperationException ex) { return BadRequest(ex.Message); }
            catch (UnauthorizedAccessException ex) { return Unauthorized(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during booking creation for DTO: {@Dto}", dto);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Gets all bookings for the currently authenticated customer.
        /// </summary>
        /// <returns>A list of the customer's bookings.</returns>
        /// <response code="200">Returns a list of the customer's bookings.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpGet("my-bookings")]
        [Authorize(Roles = Roles.Customer)]
        [ProducesResponseType(typeof(IEnumerable<BookingResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyBookings()
        {
            var customerId = GetCurrentUserId();
            var bookings = await _bookingService.GetMyBookingsAsync(customerId);
            return Ok(bookings);
        }

        // === MANAGER ENDPOINTS ===

        /// <summary>
        /// Gets active bookings for the manager's dashboard.
        /// </summary>
        /// <returns>A list of active bookings.</returns>
        /// <response code="200">Returns a list of bookings for the dashboard.</response>
        [HttpGet("dashboard")]
        [Authorize(Roles = Roles.Manager)]
        [ProducesResponseType(typeof(IEnumerable<BookingResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDashboardBookings()
        {
            var bookings = await _bookingService.GetActiveBookingsForDashboardAsync();
            return Ok(bookings);
        }

        /// <summary>
        /// Assigns a mechanic to a specific booking. Accessible only by Managers.
        /// </summary>
        /// <param name="dto">The details for the mechanic assignment.</param>
        /// <response code="204">If the mechanic was assigned successfully.</response>
        /// <response code="400">If the assignment is not allowed (e.g., booking status is incorrect).</response>
        /// <response code="404">If the booking or mechanic is not found.</response>
        [HttpPatch("assign-mechanic")]
        [Authorize(Roles = Roles.Manager)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignMechanic([FromBody] AssignMechanicRequestDto dto)
        {
            try
            {
                await _bookingService.AssignMechanicAsync(dto);
                return NoContent();
            }
            catch (BookingNotFoundException ex) { return NotFound(ex.Message); }
            catch (EmployeeNotFoundException ex) { return NotFound(ex.Message); }
            catch (MechanicAssignmentException ex) { return BadRequest(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning mechanic for BookingId {BookingId}", dto.BookingId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Cancels a booking. Accessible only by Managers.
        /// </summary>
        /// <param name="id">The ID of the booking to cancel.</param>
        /// <param name="dto">The request body containing the cancellation reason.</param>
        /// <response code="204">If the booking was cancelled successfully.</response>
        /// <response code="400">If the cancellation is not allowed.</response>
        /// <response code="404">If the booking is not found.</response>
        [HttpPatch("{id}/admin-cancel")]
        [Authorize(Roles = Roles.Manager)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelBookingByAdmin(int id, [FromBody] CancelBookingRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var adminUserId = GetCurrentUserId();
                _logger.LogInformation("Admin {AdminUserId} attempting to cancel BookingId {BookingId} with reason: {Reason}", adminUserId, id, dto.Reason);
                await _bookingService.CancelBookingByAdminAsync(id, adminUserId, dto);
                return NoContent();
            }
            catch (BookingNotFoundException ex)
            {
                _logger.LogWarning(ex, "Admin cancel failed: Booking {BookingId} not found.", id);
                return NotFound(ex.Message);
            }
            catch (BookingCancellationNotAllowedException ex)
            {
                _logger.LogWarning(ex, "Admin cancel failed for BookingId {BookingId}: Cancellation not allowed.", id);
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt in AdminCancelBooking. Message: {ErrorMessage}", ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking {BookingId} by admin.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        // === MECHANIC ENDPOINTS ===

        /// <summary>
        /// Marks a job as started. Accessible only by the assigned mechanic.
        /// </summary>
        /// <param name="id">The ID of the booking to start.</param>
        /// <response code="204">If the job was started successfully.</response>
        /// <response code="400">If the job cannot be started (e.g., wrong status).</response>
        /// <response code="403">If the mechanic is not assigned to this job.</response>
        /// <response code="404">If the booking is not found.</response>
        [HttpPost("{id}/start-job")]
        [Authorize(Roles = Roles.Mechanic)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> StartJob(int id)
        {
            try
            {
                var mechanicId = GetCurrentUserId();
                await _bookingService.StartJobAsync(id, mechanicId);
                return NoContent();
            }
            catch (BookingNotFoundException ex) { return NotFound(ex.Message); }
            catch (BookingAccessException ex) { return Forbid(ex.Message); }
            catch (JobStartConditionException ex) { return BadRequest(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting job for BookingId {BookingId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Marks a job as finished. Accessible only by the assigned mechanic.
        /// </summary>
        /// <param name="id">The ID of the booking to finish.</param>
        /// <param name="dto">The details provided by the mechanic upon finishing the job.</param>
        /// <response code="204">If the job was finished successfully.</response>
        /// <response code="400">If the job cannot be finished (e.g., wrong status).</response>
        /// <response code="403">If the mechanic is not assigned to this job.</response>
        /// <response code="404">If the booking is not found.</response>
        [HttpPost("{id}/finish-job")]
        [Authorize(Roles = Roles.Mechanic)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FinishJob(int id, [FromBody] FinishJobRequestDto dto)
        {
            try
            {
                var mechanicId = GetCurrentUserId();
                await _bookingService.FinishJobAsync(id, mechanicId, dto);
                return NoContent();
            }
            catch (BookingNotFoundException ex) { return NotFound(ex.Message); }
            catch (BookingAccessException ex) { return Forbid(ex.Message); }
            catch (JobOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finishing job for BookingId {BookingId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        // === ACCOUNTING & GENERAL ENDPOINTS ===

        /// <summary>
        /// Marks a booking as paid. Accessible by Managers or Accounts Clerks.
        /// </summary>
        /// <param name="id">The ID of the booking.</param>
        /// <param name="dto">The details of the payment.</param>
        /// <response code="204">If the booking was marked as paid successfully.</response>
        /// <response code="400">If the booking cannot be marked as paid (e.g., wrong status, already paid).</response>
        /// <response code="404">If the booking is not found.</response>
        [HttpPatch("{id}/mark-as-paid")]
        [Authorize(Roles = $"{Roles.Manager},{Roles.AccountsClerk}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkBookingAsPaid(int id, [FromBody] MarkAsPaidRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var performedByUserId = GetCurrentUserId();
                _logger.LogInformation("User {PerformedByUserId} attempting to mark BookingId {BookingId} as paid.", performedByUserId, id);
                await _bookingService.MarkAsPaidAsync(id, dto, performedByUserId);
                _logger.LogInformation("BookingId {BookingId} successfully processed for payment marking.", id);
                return NoContent();
            }
            catch (BookingNotFoundException ex)
            {
                _logger.LogWarning(ex, "Failed to mark booking {BookingId} as paid: Booking not found.", id);
                return NotFound(ex.Message);
            }
            catch (BookingOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to mark booking {BookingId} as paid: Operation not allowed. Message: {ErrorMessage}", id, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt in MarkBookingAsPaid. Message: {ErrorMessage}", ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking booking {BookingId} as paid.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while marking booking as paid.");
            }
        }

        /// <summary>
        /// Searches for bookings using various criteria. Access is role-dependent.
        /// </summary>
        /// <remarks>
        /// - Managers/Accounts Clerks can search broadly.
        /// - Customers and Mechanics can only see bookings relevant to them.
        /// This filtering is handled by the service layer.
        /// </remarks>
        /// <param name="searchParameters">The criteria to search for bookings.</param>
        /// <returns>A list of bookings matching the search criteria.</returns>
        /// <response code="200">Returns a list of matching bookings.</response>
        /// <response code="400">If search parameters are invalid (e.g., bad date format).</response>
        [HttpGet("search")]
        [Authorize(Roles = $"{Roles.Manager},{Roles.AccountsClerk},{Roles.Customer},{Roles.Mechanic}")]
        [ProducesResponseType(typeof(IEnumerable<BookingResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SearchBookings([FromQuery] BookingSearchRequestDto searchParameters)
        {
            try
            {
                // Pass the ClaimsPrincipal to the service layer so it can make decisions based on user role and ID.
                var bookings = await _bookingService.SearchBookingsAsync(searchParameters, User);
                return Ok(bookings);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Booking search failed due to invalid arguments: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Booking search failed due to unauthorized access: {ErrorMessage}", ex.Message);
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while searching bookings.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred during search.");
            }
        }
    }
}