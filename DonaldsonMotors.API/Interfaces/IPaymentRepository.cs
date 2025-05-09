using DonaldsonMotors.API.Models;


namespace DonaldsonMotors.API.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByIdAsync(int id);
        Task<IEnumerable<Payment>> ListAsync();
        Task AddAsync(Payment payment);
        void Update(Payment payment);
        void Delete(Payment payment);
        Task SaveChangesAsync();
    }
}
