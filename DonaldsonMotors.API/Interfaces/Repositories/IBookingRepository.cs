using DonaldsonMotors.API.Models;

namespace DonaldsonMotors.API.Interfaces.Repositories
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(int id);
        Task<IEnumerable<Booking>> ListAsync();
        Task AddAsync(Booking booking);
        void Update(Booking booking);
        void Delete(Booking booking);
        Task SaveChangesAsync();
    }
}
