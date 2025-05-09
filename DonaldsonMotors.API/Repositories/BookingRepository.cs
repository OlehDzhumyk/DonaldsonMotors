using DonaldsonMotors.API.Data;
using DonaldsonMotors.API.Interfaces.Repositories;
using DonaldsonMotors.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DonaldsonMotors.API.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _ctx;
        public BookingRepository(AppDbContext ctx) => _ctx = ctx;

        public async Task<Booking?> GetByIdAsync(int id) =>
            await _ctx.Bookings
                      .Include(b => b.Vehicle)
                      .Include(b => b.User)
                      .Include(b => b.Invoice)
                      .Include(b => b.Jobs)
                      .FirstOrDefaultAsync(b => b.Id == id);

        public async Task<IEnumerable<Booking>> ListAsync() =>
            await _ctx.Bookings
                      .Include(b => b.Vehicle)
                      .Include(b => b.User)
                      .ToListAsync();

        public async Task AddAsync(Booking booking) =>
            await _ctx.Bookings.AddAsync(booking);

        public void Update(Booking booking) =>
            _ctx.Bookings.Update(booking);

        public void Delete(Booking booking) =>
            _ctx.Bookings.Remove(booking);

        public Task SaveChangesAsync() =>
            _ctx.SaveChangesAsync();
    }
}
