// Interfaces/Services/IBookingService.cs
using DonaldsonMotors.API.Domain.Models;

namespace DonaldsonMotors.API.Interfaces.Services
{
    public interface IBookingService
    {
        /// <summary>
        /// Create a new booking (always sets CustomerId before calling).
        /// </summary>
        Task<Booking> CreateAsync(Booking booking);

        /// <summary>
        /// List *all* bookings in the system (for staff).
        /// </summary>
        Task<IEnumerable<Booking>> ListAllAsync();

        /// <summary>
        /// List bookings for a single customer.
        /// </summary>
        Task<IEnumerable<Booking>> ListByCustomerAsync(int customerId);

        /// <summary>
        /// Fetch a single booking by its ID.
        /// </summary>
        Task<Booking?> GetByIdAsync(int id);

        /// <summary>
        /// Update an existing booking’s editable fields.
        /// Returns false if the booking does not exist.
        /// </summary>
        Task<bool> UpdateAsync(int id, Booking updatedBooking);

        /// <summary>
        /// Delete (cancel) a booking by id.
        /// Returns false if the booking does not exist.
        /// </summary>
        Task<bool> DeleteAsync(int id);
    }
}
