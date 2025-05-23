using DonaldsonMotors.API.Data;
using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DonaldsonMotors.API.Repositories
{
    public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(AppDbContext context) : base(context) { }

        /// <summary>
        /// Checks if a mechanic has any overlapping bookings for a given time slot.
        /// </summary>
        /// <returns>True if the mechanic is available, false otherwise.</returns>
        public async Task<bool> IsMechanicAvailableAsync(int mechanicId, DateTime slotStart, double durationHours)
        {
            var slotEnd = slotStart.AddHours(durationHours);

            // A conflict exists if the mechanic has a booking where:
            // The existing booking starts before our new slot ends, AND
            // The existing booking ends after our new slot starts.
            var hasConflict = await _context.Bookings
                .AnyAsync(booking =>
                    booking.MechanicId == mechanicId &&
                    booking.SlotStart < slotEnd && // Existing starts before new ends
                    booking.SlotStart.AddHours(2) > slotStart); // Existing ends after new starts (assuming 2h duration for all bookings)

            // The mechanic is available if there is NO conflict.
            return !hasConflict;
        }
    }
}