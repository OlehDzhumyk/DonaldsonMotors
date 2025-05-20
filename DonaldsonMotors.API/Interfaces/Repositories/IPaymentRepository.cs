using DonaldsonMotors.API.Data.Entities;

namespace DonaldsonMotors.API.Interfaces.Repositories
{
    public interface IPaymentRepository
    {
        Task<PaymentEntity?> GetByIdAsync(int id);
        Task<IEnumerable<PaymentEntity>> ListAsync();
        Task AddAsync(PaymentEntity paymentEntity);
        void Update(PaymentEntity paymentEntity);
        void Delete(PaymentEntity paymentEntity);
        Task SaveChangesAsync();
    }
}