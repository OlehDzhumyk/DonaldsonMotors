// Services/BookingService.cs
using DonaldsonMotors.API.Domain.Models;
using DonaldsonMotors.API.Interfaces.Repositories;
using DonaldsonMotors.API.Interfaces.Services;

namespace DonaldsonMotors.API.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _repo;

        public BookingService(IBookingRepository repo)
        {
            _repo = repo;
        }

        public async Task<Booking> CreateAsync(Booking booking)
        {
            // business‑rule hooks (slot availability, etc.) go here...
            await _repo.AddAsync(booking);
            await _repo.SaveChangesAsync();
            return booking;
        }

        public Task<IEnumerable<Booking>> ListAllAsync()
            => _repo.ListAsync();

        public Task<IEnumerable<Booking>> ListByCustomerAsync(int customerId)
            => _repo.ListByCustomerAsync(customerId);

        public Task<Booking?> GetByIdAsync(int id)
            => _repo.GetByIdAsync(id);

        public async Task<bool> UpdateAsync(int id, Booking updatedBooking)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            // Map allowed changes (your mapper extension should handle null-checking)
            existing.BookingDate = updatedBooking.BookingDate;
            existing.Status = updatedBooking.Status;
            existing.Vehicle.Registration = updatedBooking.Vehicle.Registration;
            // …other editable props…

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
