using DonaldsonMotors.API.Interfaces;
using DonaldsonMotors.API.DTOs.Schedule;
using DonaldsonMotors.API.Mappers; // Ensure you have relevant mappers if needed here
using DonaldsonMotors.API.Data.Entities;
using Microsoft.EntityFrameworkCore; // For ToDictionaryAsync, ToHashSetAsync if applicable

namespace DonaldsonMotors.API.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ScheduleService> _logger;

        public ScheduleService(IUnitOfWork unitOfWork, ILogger<ScheduleService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<DateTime>> GetAvailabilityAsync(DateTime startDateUtc, DateTime endDateUtc) // Parameters are now explicitly UTC
        {
            _logger.LogInformation("Calculating availability from UTC {StartDateUtc} to UTC {EndDateUtc}", startDateUtc, endDateUtc);

            var workingHoursList = await _unitOfWork.Schedule.GetWorkingHoursAsync();
            var settings = await _unitOfWork.Schedule.GetSettingsAsync();
            var exceptionsList = await _unitOfWork.Schedule.GetExceptionsAsync();

            // The parameters startDateUtc and endDateUtc are already Kind=Utc
            // We query bookings up to, but not including, the start of the day *after* endDateUtc.
            var queryEndDate = endDateUtc.AddDays(1);
            var bookingsList = await _unitOfWork.Bookings.FindAsync(b =>
                b.SlotStart >= startDateUtc &&
                b.SlotStart < queryEndDate &&
                b.Status != BookingStatus.Cancelled);

            var workingHours = workingHoursList.ToDictionary(wh => wh.DayOfWeek);
            var exceptions = new HashSet<DateOnly>(exceptionsList.Select(e => e.Date));
            var bookedSlots = new HashSet<DateTime>(bookingsList.Select(b => b.SlotStart));

            var availableSlots = new List<DateTime>();
            const int slotDurationHours = 2;

            // Loop through each day in the UTC range
            for (var day = startDateUtc; day < queryEndDate; day = day.AddDays(1)) // Iterate up to queryEndDate
            {
                // The 'day' variable here is already UTC because startDateUtc is UTC.
                if (exceptions.Contains(DateOnly.FromDateTime(day)))
                    continue;

                if (!workingHours.TryGetValue(day.DayOfWeek, out var hoursForDay))
                    continue;

                for (var time = hoursForDay.StartTime;
                     time.AddHours(slotDurationHours) <= hoursForDay.EndTime;
                     time = time.AddHours(slotDurationHours))
                {
                    // Construct the potential slot. Since 'day' is UTC, 'potentialSlot' will also be UTC.
                    var potentialSlot = day.Add(time.ToTimeSpan());

                    // Compare with DateTime.UtcNow (which is always UTC)
                    if (potentialSlot < DateTime.UtcNow)
                        continue;

                    var potentialSlotEndLocalTime = time.AddHours(slotDurationHours); // Local time for comparison with lunch
                    if (settings != null &&
                        potentialSlotEndLocalTime > settings.LunchStartTime &&
                        time < settings.LunchEndTime)
                        continue;

                    // bookedSlots contains UTC DateTimes from the database
                    if (bookedSlots.Contains(potentialSlot))
                        continue;

                    availableSlots.Add(potentialSlot);
                }
            }

            _logger.LogInformation("Found {Count} available slots from UTC {StartDateUtc} to UTC {EndDateUtc}.", availableSlots.Count, startDateUtc, endDateUtc);
            return availableSlots.OrderBy(s => s);
        }

        // --- Other ScheduleService methods (UpdateWorkingHoursAsync, etc.) ---
        // These methods already use _unitOfWork.CompleteAsync() after their respective repository calls,
        // which is correct as they are individual operations.
        #region Manager Methods
        public async Task UpdateWorkingHoursAsync(IEnumerable<WorkingDayDto> workingDaysDto)
        {
            _logger.LogInformation("Updating working hours.");

            var workingHoursEntities = new List<WorkingHours>();
            foreach (var dayDto in workingDaysDto)
            {
                if (!TimeOnly.TryParse(dayDto.StartTime, out var startTime) || !TimeOnly.TryParse(dayDto.EndTime, out var endTime))
                {
                    throw new ArgumentException($"Invalid time format for {dayDto.DayOfWeek}. Must be HH:mm");
                }
                if (startTime >= endTime)
                {
                    throw new ArgumentException($"StartTime must be before EndTime for {dayDto.DayOfWeek}.");
                }
                workingHoursEntities.Add(new WorkingHours { DayOfWeek = dayDto.DayOfWeek, StartTime = startTime, EndTime = endTime });
            }
            await _unitOfWork.Schedule.UpdateWorkingHoursAsync(workingHoursEntities);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Successfully updated working hours.");
        }

        public async Task UpdateLunchBreakAsync(LunchBreakDto lunchBreakDto)
        {
            _logger.LogInformation("Updating lunch break time.");
            if (!TimeOnly.TryParse(lunchBreakDto.StartTime, out var startTime) || !TimeOnly.TryParse(lunchBreakDto.EndTime, out var endTime))
            {
                throw new ArgumentException("Invalid time format for lunch break. Must be HH:mm");
            }
            if (startTime >= endTime)
            {
                throw new ArgumentException("Lunch StartTime must be before EndTime.");
            }

            var settings = await _unitOfWork.Schedule.GetSettingsAsync() ?? new ScheduleSettings { Id = 1 }; // Ensure Id is set if new
            settings.LunchStartTime = startTime;
            settings.LunchEndTime = endTime;

            _unitOfWork.Schedule.UpdateSettings(settings);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Successfully updated lunch break time.");
        }

        public async Task<ScheduleExceptionResponseDto> AddScheduleExceptionAsync(CreateScheduleExceptionDto exceptionDto)
        {
            _logger.LogInformation("Adding schedule exception for Date: {Date}, Description: {Description}", exceptionDto.Date, exceptionDto.Description);
            var exceptionEntity = exceptionDto.ToScheduleExceptionEntity();
            await _unitOfWork.Schedule.AddExceptionAsync(exceptionEntity);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Successfully added schedule exception with ID {Id}", exceptionEntity.Id);
            return exceptionEntity.ToScheduleExceptionResponseDto();
        }

        public async Task DeleteScheduleExceptionAsync(int id)
        {
            _logger.LogInformation("Deleting schedule exception with ID {Id}", id);
            var exception = await _unitOfWork.Schedule.GetExceptionByIdAsync(id)
                ?? throw new KeyNotFoundException($"Schedule exception with ID {id} not found.");
            _unitOfWork.Schedule.DeleteException(exception);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Successfully deleted schedule exception with ID {Id}", id);
        }

        public async Task<IEnumerable<ScheduleExceptionResponseDto>> GetScheduleExceptionsAsync()
        {
            _logger.LogInformation("Fetching all schedule exceptions.");
            var exceptions = await _unitOfWork.Schedule.GetExceptionsAsync();
            return exceptions.Select(e => e.ToScheduleExceptionResponseDto());
        }
        #endregion
    }
}