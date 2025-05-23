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
                .Include(b => b.Vehicle)
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
                .Include(b => b.Vehicle)
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
                .Include(b => b.Vehicle)
                .OrderBy(b => b.SlotStart)
                .ToListAsync();
        }
    }
}