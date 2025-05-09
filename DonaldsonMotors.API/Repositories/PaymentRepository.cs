using DonaldsonMotors.API.Data;
using DonaldsonMotors.API.Interfaces.Repositories;
using DonaldsonMotors.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DonaldsonMotors.API.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _ctx;
        public PaymentRepository(AppDbContext ctx) => _ctx = ctx;

        public async Task<Payment?> GetByIdAsync(int id) =>
            await _ctx.Payments
                      .Include(p => p.Invoice)
                      .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IEnumerable<Payment>> ListAsync() =>
            await _ctx.Payments.ToListAsync();

        public async Task AddAsync(Payment payment) =>
            await _ctx.Payments.AddAsync(payment);

        public void Update(Payment payment) =>
            _ctx.Payments.Update(payment);

        public void Delete(Payment payment) =>
            _ctx.Payments.Remove(payment);

        public Task SaveChangesAsync() =>
            _ctx.SaveChangesAsync();
    }
}
