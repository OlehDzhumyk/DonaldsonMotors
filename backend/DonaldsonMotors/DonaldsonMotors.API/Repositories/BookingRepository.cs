using DonaldsonMotors.API.Data;
using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DonaldsonMotors.API.Repositories
{
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        public BookingRepository(AppDbContext context) : base(context) { }

        public async Task<Booking?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Mechanic)
                .Include(b => b.Vehicle)
                .Include(b => b.Invoice)
                .Include(b => b.ServiceType)
                .Include(b => b.Jobs)!
                    .ThenInclude(j => j.JobParts)!
                    .ThenInclude(jp => jp.Part)
                .FirstOrDefaultAsync(b => b.Id == id);
        }


      

        public async Task<IEnumerable<Booking>> GetBookingsByCustomerIdAsync(int customerId)
        {
            return await _context.Bookings
                .Where(b => b.CustomerId == customerId)
                .Include(b => b.Customer)
                .Include(b => b.Vehicle)
                .Include(b => b.Mechanic)
                .Include(b => b.ServiceType)
                .OrderByDescending(b => b.SlotStart)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetDashboardBookingsAsync()
        {
            var activeStatuses = new[] { BookingStatus.Pending, BookingStatus.Assigned, BookingStatus.InProgress, BookingStatus.AwaitingPayment };
            return await _context.Bookings
                .Where(b => activeStatuses.Contains(b.Status))
                .Include(b => b.Customer)
                .Include(b => b.Mechanic)
                .Include(b => b.Vehicle)
                .Include(b => b.ServiceType)
                .OrderBy(b => b.SlotStart)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> SearchBookingsAsync(
    int? customerId,
    string? vehicleRegistration,
    int? mechanicId,
    DateTime? dateFrom,
    DateTime? dateTo,
    BookingStatus? status)
        {
            // Start with a queryable that includes all necessary related data for BookingResponseDto
            var query = _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Vehicle)
                .Include(b => b.ServiceType)
                .Include(b => b.Mechanic) // Assuming you added Mechanic navigation property to Booking
                .AsQueryable();

            if (customerId.HasValue)
            {
                query = query.Where(b => b.CustomerId == customerId.Value);
            }

            if (!string.IsNullOrWhiteSpace(vehicleRegistration))
            {
                query = query.Where(b => b.VehicleRegistrationNumber.ToUpper() == vehicleRegistration.ToUpper());
            }

            if (mechanicId.HasValue)
            {
                query = query.Where(b => b.MechanicId == mechanicId.Value);
            }

            if (dateFrom.HasValue)
            {
                // Ensure we compare date parts or use UTC consistently
                var utcDateFrom = DateTime.SpecifyKind(dateFrom.Value.Date, DateTimeKind.Utc);
                query = query.Where(b => b.SlotStart >= utcDateFrom);
            }

            if (dateTo.HasValue)
            {
                var utcDateTo = DateTime.SpecifyKind(dateTo.Value.Date, DateTimeKind.Utc).AddDays(1); // Exclusive end
                query = query.Where(b => b.SlotStart < utcDateTo);
            }

            if (status.HasValue)
            {
                query = query.Where(b => b.Status == status.Value);
            }

            // Order by slot start, most recent first, for example
            return await query.OrderByDescending(b => b.SlotStart).ToListAsync();
        }


    }
}