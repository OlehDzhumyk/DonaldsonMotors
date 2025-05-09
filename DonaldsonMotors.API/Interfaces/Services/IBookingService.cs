using DonaldsonMotors.API.Models;

namespace DonaldsonMotors.API.Interfaces
{
    public interface IBookingService
    {
        Task<IEnumerable<Booking>> GetAllAsync();
        Task<Booking?> GetByIdAsync(int id);
        Task<Booking> CreateAsync(Booking booking);
        Task<bool> UpdateAsync(int id, Booking booking);
        Task<bool> DeleteAsync(int id);
    }
}
