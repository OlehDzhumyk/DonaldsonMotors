using Microsoft.AspNetCore.Mvc;
using DonaldsonMotors.API.Interfaces;
using DonaldsonMotors.API.DTOs.Schedule;
using Microsoft.AspNetCore.Authorization;
using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Exceptions;

namespace DonaldsonMotors.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;
        private readonly ILogger<ScheduleController> _logger;

        public ScheduleController(IScheduleService scheduleService, ILogger<ScheduleController> logger)
        {
            _scheduleService = scheduleService;
            _logger = logger;
        }

        /// <summary>
        /// Gets a list of available booking slots for a given date range. This is publicly accessible.
        /// </summary>
        /// <param name="startDate">The start of the date range (e.g., 2025-06-01). The time part is ignored.</param>
        /// <param name="endDate">The end of the date range (e.g., 2025-06-08). The time part is ignored.</param>
        /// <returns>A list of available booking slots as UTC DateTimes.</returns>
        /// <response code="200">Returns a list of available UTC time slots.</response>
        /// <response code="400">If the date range is invalid (e.g., start date is after end date or range is too long).</response>
        [HttpGet("availability")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<DateTime>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAvailability([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            // Treat incoming date parameters as dates only, in UTC.
            var utcStartDate = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
            var utcEndDate = DateTime.SpecifyKind(endDate.Date, DateTimeKind.Utc);

            if (utcStartDate >= utcEndDate)
                return Problem(detail: "Start date must be before end date.", statusCode: StatusCodes.Status400BadRequest);

            if ((utcEndDate - utcStartDate).TotalDays > 60)
                return Problem(detail: "The date range cannot be longer than 60 days.", statusCode: StatusCodes.Status400BadRequest);

            _logger.LogInformation("Fetching availability from {UtcStartDate} to {UtcEndDate}", utcStartDate, utcEndDate);
            var availableSlots = await _scheduleService.GetAvailabilityAsync(utcStartDate, utcEndDate);
            return Ok(availableSlots);
        }

        // --- MANAGER-ONLY ENDPOINTS ---

        /// <summary>
        /// Gets a list of mechanics and their availability for a specific booking slot. For manager use.
        /// </summary>
        /// <param name="slotStart">The UTC start time of the slot to check (e.g., '2025-07-20T10:00:00Z').</param>
        /// <param name="serviceTypeId">The ID of the service type, used to determine the required duration.</param>
        /// <returns>A list of mechanics and their availability status for the given slot.</returns>
        /// <response code="200">Returns the list of mechanics and their availability.</response>
        /// <response code="400">If the service type ID is invalid.</response>
        /// <response code="404">If the specified service type is not found.</response>
        [HttpGet("available-mechanics")]
        [Authorize(Roles = Roles.Manager)]
        [ProducesResponseType(typeof(IEnumerable<MechanicAvailabilityDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAvailableMechanicsForSlot([FromQuery] DateTime slotStart, [FromQuery] int serviceTypeId)
        {
            // Ensure the DateTime Kind is UTC, as expected by the service layer.
            var utcSlotStart = DateTime.SpecifyKind(slotStart, DateTimeKind.Utc);

            if (serviceTypeId <= 0)
                return Problem(detail: "A valid Service Type ID is required.", statusCode: StatusCodes.Status400BadRequest);

            _logger.LogInformation("Manager requesting available mechanics for slot {UtcSlotStart} and ServiceType {ServiceTypeId}", utcSlotStart, serviceTypeId);
            var availableMechanics = await _scheduleService.GetAvailableMechanicsForSlotAsync(utcSlotStart, serviceTypeId);
            return Ok(availableMechanics);
        }

        /// <summary>
        /// Updates the garage's standard weekly working hours. Accessible only by Managers.
        /// </summary>
        /// <param name="workingDays">A list of working days with their start and end times.</param>
        /// <response code="204">If the working hours were updated successfully.</response>
        /// <response code="400">If the provided data is invalid (e.g., end time is before start time).</response>
        [HttpPut("working-hours")]
        [Authorize(Roles = Roles.Manager)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateWorkingHours([FromBody] IEnumerable<WorkingDayDto> workingDays)
        {
            await _scheduleService.UpdateWorkingHoursAsync(workingDays);
            return NoContent();
        }

        /// <summary>
        /// Updates the garage's standard daily lunch break. Accessible only by Managers.
        /// </summary>
        /// <param name="lunchBreak">The start time and duration of the lunch break.</param>
        /// <response code="204">If the lunch break was updated successfully.</response>
        /// <response code="400">If the provided data is invalid.</response>
        [HttpPut("lunch-break")]
        [Authorize(Roles = Roles.Manager)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateLunchBreak([FromBody] LunchBreakDto lunchBreak)
        {
            await _scheduleService.UpdateLunchBreakAsync(lunchBreak);
            return NoContent();
        }

        /// <summary>
        /// Gets all configured schedule exceptions (e.g., holidays, special closures). Accessible only by Managers.
        /// </summary>
        /// <returns>A list of schedule exceptions.</returns>
        /// <response code="200">Returns the list of schedule exceptions.</response>
        [HttpGet("exceptions")]
        [Authorize(Roles = Roles.Manager)]
        [ProducesResponseType(typeof(IEnumerable<ScheduleExceptionResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetExceptions()
        {
            var exceptions = await _scheduleService.GetScheduleExceptionsAsync();
            return Ok(exceptions);
        }

        /// <summary>
        /// Adds a new schedule exception (e.g., a public holiday). Accessible only by Managers.
        /// </summary>
        /// <param name="exceptionDto">The details of the schedule exception to create.</param>
        /// <returns>The created schedule exception.</returns>
        /// <response code="201">Returns the newly created exception.</response>
        /// <response code="400">If the provided data is invalid.</response>
        [HttpPost("exceptions")]
        [Authorize(Roles = Roles.Manager)]
        [ProducesResponseType(typeof(ScheduleExceptionResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddException([FromBody] CreateScheduleExceptionDto exceptionDto)
        {
            var createdException = await _scheduleService.AddScheduleExceptionAsync(exceptionDto);
            return CreatedAtAction(nameof(GetExceptions), new { id = createdException.Id }, createdException);
        }

        /// <summary>
        /// Deletes a schedule exception. Accessible only by Managers.
        /// </summary>
        /// <param name="id">The ID of the schedule exception to delete.</param>
        /// <response code="204">If the exception was deleted successfully.</response>
        /// <response code="404">If an exception with the specified ID is not found.</response>
        [HttpDelete("exceptions/{id}")]
        [Authorize(Roles = Roles.Manager)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteException(int id)
        {
            await _scheduleService.DeleteScheduleExceptionAsync(id);
            return NoContent();
        }
    }
}