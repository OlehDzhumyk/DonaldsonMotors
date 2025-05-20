using DonaldsonMotors.API.Data;
using DonaldsonMotors.API.Domain.Models;
using DonaldsonMotors.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Mappers;

namespace DonaldsonMotors.API.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _ctx;

        public BookingRepository(AppDbContext ctx) => _ctx = ctx;

        public async Task<Booking?> GetByIdAsync(int id)
        {
            var entity = await _ctx.Bookings
                .Include(b => b.Vehicle)
                .Include(b => b.Customer)
                .Include(b => b.Invoice)
                .Include(b => b.Jobs)
                .FirstOrDefaultAsync(b => b.Id == id);

            return entity?.ToDomainModel();
        }

        public async Task<IEnumerable<Booking>> ListAsync()
        {
            var entities = await _ctx.Bookings
                .Include(b => b.Vehicle)
                .Include(b => b.Customer)
                .ToListAsync();

            return entities.Select(e => e.ToDomainModel());
        }

        public async Task AddAsync(Booking booking)
        {
            var entity = booking.ToEntity();
            await _ctx.Bookings.AddAsync(entity);
        }

        public void Update(Booking booking)
        {
            var entity = booking.ToEntity();
            _ctx.Bookings.Update(entity);
        }

        public void Delete(Booking booking)
        {
            var entity = booking.ToEntity();
            _ctx.Bookings.Remove(entity);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _ctx.SaveChangesAsync();
        }

        public async Task<IEnumerable<Booking>> ListByCustomerAsync(int customerId)
        {
            var entities = await _ctx.Bookings
                .Where(b => b.CustomerId == customerId)
                .Include(b => b.Vehicle)
                .Include(b => b.Customer)
                .ToListAsync();

            return entities.Select(e => e.ToDomainModel());
        }
    }
}