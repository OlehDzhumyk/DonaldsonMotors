using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.DTOs.Schedule;
using DonaldsonMotors.API.Exceptions; // Ensure you have ServiceTypeNotFoundException etc.
using DonaldsonMotors.API.Interfaces;
using DonaldsonMotors.API.Mappers; // For EmployeeMapper
using Microsoft.AspNetCore.Identity;

namespace DonaldsonMotors.API.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ScheduleService> _logger;

        public ScheduleService(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            ILogger<ScheduleService> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Calculates and returns a list of available booking slots for customers.
        /// A slot is available if it's within general working hours, not a general holiday, and not during general lunch.
        /// This version DOES NOT check any existing bookings or mechanic availability/capacity.
        /// </summary>
        public async Task<IEnumerable<DateTime>> GetAvailabilityAsync(DateTime startDateUtc, DateTime endDateUtc)
        {
            _logger.LogInformation("[SIMPLIFIED] Calculating general availability from {StartDateUtc} to {EndDateUtc} (ignores existing bookings and mechanic capacity)",
                startDateUtc, endDateUtc);

            var workingHoursList = await _unitOfWork.Schedule.GetWorkingHoursAsync();
            var settings = await _unitOfWork.Schedule.GetSettingsAsync()
                ?? new ScheduleSettings { Id = 1, LunchStartTime = new TimeOnly(13, 0), LunchEndTime = new TimeOnly(14, 0) }; // Default if not configured
            var exceptionsList = await _unitOfWork.Schedule.GetExceptionsAsync();
            var queryEndDate = endDateUtc.Date.AddDays(1);

            var workingHoursMap = workingHoursList.ToDictionary(wh => wh.DayOfWeek);
            var exceptionsSet = new HashSet<DateOnly>(exceptionsList.Select(e => e.Date));

            var availableSlots = new List<DateTime>();
            const int slotDurationHours = 2; // Default 2-hour slots for display

            for (var day = startDateUtc.Date; day < queryEndDate; day = day.AddDays(1))
            {
                if (exceptionsSet.Contains(DateOnly.FromDateTime(day)))
                {
                    _logger.LogDebug("Day {Day} is a general exception. Skipping.", day);
                    continue;
                }

                if (!workingHoursMap.TryGetValue(day.DayOfWeek, out var hoursForDay))
                {
                    _logger.LogDebug("No general working hours defined for {DayOfWeek}. Skipping.", day.DayOfWeek);
                    continue;
                }

                _logger.LogDebug("Checking day {Day}, General Working Hours: {StartTime} - {EndTime}, General Lunch: {LunchStart} - {LunchEnd}",
                    day, hoursForDay.StartTime, hoursForDay.EndTime, settings.LunchStartTime, settings.LunchEndTime);

                for (var time = hoursForDay.StartTime;
                     time.AddHours(slotDurationHours) <= hoursForDay.EndTime;
                     time = time.AddHours(slotDurationHours))
                {
                    var potentialSlotStartTime = day.Add(time.ToTimeSpan());
                    potentialSlotStartTime = DateTime.SpecifyKind(potentialSlotStartTime, DateTimeKind.Utc);
                    var potentialSlotEndTime = potentialSlotStartTime.AddHours(slotDurationHours);

                    if (potentialSlotStartTime < DateTime.UtcNow)
                    {
                        _logger.LogDebug("Skipping past slot: {PotentialSlotStartTime}", potentialSlotStartTime);
                        continue;
                    }

                    // Check for general lunch break overlap
                    if (potentialSlotStartTime.TimeOfDay < settings.LunchEndTime.ToTimeSpan() &&
                        potentialSlotEndTime.TimeOfDay > settings.LunchStartTime.ToTimeSpan())
                    {
                        _logger.LogDebug("Skipping slot {PotentialSlotStartTime} due to general lunch break overlap.", potentialSlotStartTime);
                        continue;
                    }

                    // SIMPLIFIED LOGIC: If it passes garage open/lunch/holiday checks, it's "available"
                    availableSlots.Add(potentialSlotStartTime);
                }
            }

            _logger.LogInformation("Found {Count} generally available slots based purely on garage schedule.", availableSlots.Count);
            return availableSlots.OrderBy(s => s);
        }

        /// <summary>
        /// Gets a list of all mechanics and checks their existing bookings for conflicts with a specific job slot.
        /// Assumes the provided slotStartUtc is already validated against general garage working hours/holidays/lunch.
        /// For manager use when assigning a mechanic to a booking.
        /// </summary>
        public async Task<IEnumerable<MechanicAvailabilityDto>> GetAvailableMechanicsForSlotAsync(DateTime slotStartUtc, int serviceTypeId)
        {
            _logger.LogInformation("Fetching available mechanics for slot {SlotStartUtc} and ServiceTypeID {ServiceTypeId} (simplified check - only for booking conflicts)",
                slotStartUtc, serviceTypeId);

            var serviceType = await _unitOfWork.ServiceTypes.GetByIdAsync(serviceTypeId)
                ?? throw new ServiceTypeNotFoundException(serviceTypeId);

            var slotDurationHours = serviceType.DurationHours;
            var slotEndUtc = slotStartUtc.AddHours(slotDurationHours);

            // Step 1: Get all active mechanics
            var usersInMechanicRole = await _userManager.GetUsersInRoleAsync(Roles.Mechanic);
            var allMechanics = new List<Employee>();
            if (usersInMechanicRole != null)
            {
                foreach (var userInRole in usersInMechanicRole)
                {
                    var employee = await _unitOfWork.Users.GetEmployeeByIdAsync(userInRole.Id);
                    if (employee != null)
                    {
                        allMechanics.Add(employee);
                    }
                    else
                    {
                        _logger.LogWarning("User {UserId} is in Mechanic role but not found as an Employee type when checking availability.", userInRole.Id);
                    }
                }
            }

            var availabilityResults = new List<MechanicAvailabilityDto>();

            if (!allMechanics.Any())
            {
                _logger.LogWarning("No mechanics found in the system to check availability for slot {SlotStartUtc}.", slotStartUtc);
                return availabilityResults; // Return empty list if no mechanics
            }

            // Step 2: For each mechanic, check only their existing bookings for conflicts
            foreach (var mechanic in allMechanics)
            {
                // Get bookings for this mechanic on the date of the slotStart.
                var mechanicBookingsOnDate = await _unitOfWork.Users.GetMechanicBookingsOnDateAsync(mechanic.Id, slotStartUtc.Date);

                var isAvailable = true;
                var reason = "Booked on another job";

                if (mechanicBookingsOnDate.Any())
                {
                    foreach (var existingBooking in mechanicBookingsOnDate)
                    {
                        var existingBookingStart = existingBooking.SlotStart;
                        var existingBookingDuration = existingBooking.ServiceType?.DurationHours ?? 2.0; // Fallback
                        var existingBookingEnd = existingBookingStart.AddHours(existingBookingDuration);

                        // Standard overlap condition: (NewSlotStart < ExistingBookingEnd) AND (NewSlotEnd > ExistingBookingStart)
                        if (slotStartUtc < existingBookingEnd && slotEndUtc > existingBookingStart)
                        {
                            isAvailable = false;
                            reason = $"Booked on job ID {existingBooking.Id} ({existingBookingStart:HH:mm}-{existingBookingEnd:HH:mm})";
                            _logger.LogDebug("Mechanic {MechanicId} ({MechanicName}) conflicts with existing booking {ExistingBookingId} for new slot {SlotStartUtc}",
                                mechanic.Id, mechanic.FullName, existingBooking.Id, slotStartUtc);
                            break;
                        }
                    }
                }
                availabilityResults.Add(mechanic.ToMechanicAvailabilityDto(isAvailable, isAvailable ? null : reason));
            }

            _logger.LogInformation("Checked availability for {NumMechanics} mechanics for slot {SlotStartUtc} - {SlotEndUtc}.", availabilityResults.Count, slotStartUtc, slotEndUtc);
            return availabilityResults;
        }   

        #region Manager Schedule Setting Methods (Unchanged from your last version)
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
            var settings = await _unitOfWork.Schedule.GetSettingsAsync() ?? new ScheduleSettings { Id = 1 };
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

        public async Task<IEnumerable<WorkingDayDto>> GetGarageWorkingHoursAsync()
        {
            var hours = await _unitOfWork.Schedule.GetWorkingHoursAsync();
            return hours.Select(h => new WorkingDayDto
            {
                DayOfWeek = h.DayOfWeek,
                StartTime = h.StartTime.ToString("HH':'mm"),
                EndTime = h.EndTime.ToString("HH':'mm")
            }).ToList();
        }

        public async Task<LunchBreakDto?> GetGarageLunchBreakAsync()
        {
            var settings = await _unitOfWork.Schedule.GetSettingsAsync();
            if (settings == null) return null;
            return new LunchBreakDto
            {
                StartTime = settings.LunchStartTime.ToString("HH':'mm"),
                EndTime = settings.LunchEndTime.ToString("HH':'mm")
            };
        }
        #endregion
    }
}