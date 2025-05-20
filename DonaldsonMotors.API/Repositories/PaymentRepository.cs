using DonaldsonMotors.API.Data;
using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DonaldsonMotors.API.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _ctx;
        public PaymentRepository(AppDbContext ctx) => _ctx = ctx;

        public async Task<PaymentEntity?> GetByIdAsync(int id) =>
            await _ctx.Payments
                .Include(p => p.Invoice)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IEnumerable<PaymentEntity>> ListAsync() =>
            await _ctx.Payments.ToListAsync();

        public async Task AddAsync(PaymentEntity paymentEntity) =>
            await _ctx.Payments.AddAsync(paymentEntity);

        public void Update(PaymentEntity paymentEntity) =>
            _ctx.Payments.Update(paymentEntity);

        public void Delete(PaymentEntity paymentEntity) =>
            _ctx.Payments.Remove(paymentEntity);

        public async Task SaveChangesAsync() =>
            await _ctx.SaveChangesAsync();
    }
}