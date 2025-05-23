using Microsoft.AspNetCore.Mvc;
using DonaldsonMotors.API.Interfaces;
using DonaldsonMotors.API.DTOs.Schedule;
using Microsoft.AspNetCore.Authorization;
using DonaldsonMotors.API.Data.Entities;

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
        /// Gets a list of available booking slots for a given date range.
        /// </summary>
        /// <param name="startDate">The start of the range (date part is used), e.g., 2025-06-01. Assumed to be UTC.</param>
        /// <param name="endDate">The end of the range (date part is used), e.g., 2025-06-08. Assumed to be UTC.</param>
        /// <returns>A list of available DateTimes in UTC.</returns>
        [HttpGet("availability")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<DateTime>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAvailability([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            // Ensure the Kind is UTC for the dates passed to the service
            // We use .Date to only consider the date part, then specify it as UTC.
            // This means we are looking for slots starting from 00:00 UTC on startDate
            // up to (but not including) 00:00 UTC on endDate.AddDays(1).
            var utcStartDate = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
            var utcEndDate = DateTime.SpecifyKind(endDate.Date, DateTimeKind.Utc);

            if (utcStartDate >= utcEndDate)
            {
                return BadRequest("Start date must be before end date.");
            }
            if ((utcEndDate - utcStartDate).TotalDays > 60) // Max 60 days range
            {
                return BadRequest("The date range cannot be longer than 60 days.");
            }

            _logger.LogInformation("Fetching availability from {UtcStartDate} to {UtcEndDate}", utcStartDate, utcEndDate);
            var availableSlots = await _scheduleService.GetAvailabilityAsync(utcStartDate, utcEndDate);
            return Ok(availableSlots);
        }

        // --- EXISTING MANAGER-ONLY ENDPOINTS ---
        // (No changes needed here unless they also pass DateTime.Unspecified to services for DB queries)
        #region Manager Endpoints
        [HttpPut("working-hours")]
        [Authorize(Roles = Roles.Manager)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateWorkingHours([FromBody] IEnumerable<WorkingDayDto> workingDays)
        {
            try
            {
                await _scheduleService.UpdateWorkingHoursAsync(workingDays);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("lunch-break")]
        [Authorize(Roles = Roles.Manager)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateLunchBreak([FromBody] LunchBreakDto lunchBreak)
        {
            try
            {
                await _scheduleService.UpdateLunchBreakAsync(lunchBreak);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("exceptions")]
        [Authorize(Roles = Roles.Manager)]
        [ProducesResponseType(typeof(IEnumerable<ScheduleExceptionResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetExceptions()
        {
            var exceptions = await _scheduleService.GetScheduleExceptionsAsync();
            return Ok(exceptions);
        }

        [HttpPost("exceptions")]
        [Authorize(Roles = Roles.Manager)]
        [ProducesResponseType(typeof(ScheduleExceptionResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddException([FromBody] CreateScheduleExceptionDto exceptionDto)
        {
            // Ensure the DTO's DateTime is handled correctly if it could be Unspecified
            // One way is to convert it to UTC if your business logic assumes UTC for exceptions
            // For simplicity, if your DTO's Date is for a whole day, DateOnly in the entity is better.
            // If the DTO's Date is specific, ensure its Kind is set before passing to service,
            // or handle in service/mapper.
            // Assuming CreateScheduleExceptionDto's Date is already handled (e.g. DateOnly internally or client sends UTC)

            var createdException = await _scheduleService.AddScheduleExceptionAsync(exceptionDto);
            return CreatedAtAction(nameof(GetExceptions), new { id = createdException.Id }, createdException);
        }

        [HttpDelete("exceptions/{id}")]
        [Authorize(Roles = Roles.Manager)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteException(int id)
        {
            try
            {
                await _scheduleService.DeleteScheduleExceptionAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
        #endregion
    }
}