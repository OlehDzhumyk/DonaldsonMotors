using DonaldsonMotors.API.Interfaces;
using DonaldsonMotors.API.Interfaces.Repositories;
using DonaldsonMotors.API.Models;

namespace DonaldsonMotors.API.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _repo;

        public BookingService(IBookingRepository repo)
            => _repo = repo;

        public async Task<IEnumerable<Booking>> GetAllAsync()
            => await _repo.ListAsync();

        public async Task<Booking?> GetByIdAsync(int id)
            => await _repo.GetByIdAsync(id);

        public async Task<Booking> CreateAsync(Booking booking)
        {
            // example business rule:
            // if (!await SlotAvailable(booking.BookingDate)) throw ...
            await _repo.AddAsync(booking);
            await _repo.SaveChangesAsync();
            return booking;
        }

        public async Task<bool> UpdateAsync(int id, Booking booking)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;
            existing.BookingDate = booking.BookingDate;
            existing.Status = booking.Status;
            _repo.Update(existing);
            await _repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;
            _repo.Delete(existing);
            await _repo.SaveChangesAsync();
            return true;
        }
    }
}
