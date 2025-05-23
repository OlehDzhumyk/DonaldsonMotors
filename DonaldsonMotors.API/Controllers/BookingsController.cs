using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.DTOs.Booking;
using DonaldsonMotors.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DonaldsonMotors.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All endpoints in this controller require authentication by default
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(IBookingService bookingService, ILogger<BookingsController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        // ====================================================================
        // CUSTOMER ENDPOINTS
        // ====================================================================

        /// <summary>
        /// Creates a new booking. Accessible only by customers.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = Roles.Customer)]
        [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequestDto dto)
        {
            // Get the authenticated user's ID from the JWT token claims
            var customerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var createdBooking = await _bookingService.CreateBookingAsync(customerId, dto);
                // Returns a 201 Created status with the location of the new resource (optional but good practice)
                return CreatedAtAction(nameof(GetMyBookings), null, createdBooking);
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex is ArgumentException)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                // This typically happens if the slot is taken (race condition)
                return Conflict(ex.Message);
            }
        }

        /// <summary>
        /// Gets all bookings for the currently authenticated customer.
        /// </summary>
        [HttpGet("my-bookings")]
        [Authorize(Roles = Roles.Customer)]
        [ProducesResponseType(typeof(IEnumerable<BookingResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyBookings()
        {
            var customerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var bookings = await _bookingService.GetMyBookingsAsync(customerId);
            return Ok(bookings);
        }

        /// <summary>
        /// Cancels a booking. Accessible only by the customer who owns the booking.
        /// </summary>
        [HttpPatch("{id}/cancel")]
        [Authorize(Roles = Roles.Customer)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var customerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            try
            {
                await _bookingService.CancelBookingAsync(id, customerId);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        // ====================================================================
        // MANAGER ENDPOINTS
        // ====================================================================

        /// <summary>
        /// Gets all active bookings for the manager's dashboard.
        /// </summary>
        [HttpGet("dashboard")]
        [Authorize(Roles = Roles.Manager)]
        [ProducesResponseType(typeof(IEnumerable<BookingResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDashboardBookings()
        {
            var bookings = await _bookingService.GetActiveBookingsForDashboardAsync();
            return Ok(bookings);
        }

        /// <summary>
        /// Assigns a mechanic to a pending booking.
        /// </summary>
        [HttpPut("assign-mechanic")]
        [Authorize(Roles = Roles.Manager)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignMechanic([FromBody] AssignMechanicRequestDto dto)
        {
            try
            {
                await _bookingService.AssignMechanicAsync(dto);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        // ====================================================================
        // MECHANIC ENDPOINTS
        // ====================================================================

        /// <summary>
        /// Marks a job as started. Accessible by the assigned mechanic.
        /// </summary>
        [HttpPost("{id}/start-job")]
        [Authorize(Roles = Roles.Mechanic)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> StartJob(int id)
        {
            var mechanicId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            try
            {
                await _bookingService.StartJobAsync(id, mechanicId);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        /// <summary>
        /// Marks a job as finished, providing work details and used parts.
        /// </summary>
        [HttpPost("{id}/finish-job")]
        [Authorize(Roles = Roles.Mechanic)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> FinishJob(int id, [FromBody] FinishJobRequestDto dto)
        {
            var mechanicId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            try
            {
                await _bookingService.FinishJobAsync(id, mechanicId, dto);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}