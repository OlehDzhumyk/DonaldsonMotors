using DonaldsonMotors.API.Data;
using DonaldsonMotors.API.Interfaces.Repositories;
using DonaldsonMotors.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DonaldsonMotors.API.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly AppDbContext _ctx;
        public InvoiceRepository(AppDbContext ctx) => _ctx = ctx;

        public async Task<Invoice?> GetByIdAsync(int id) =>
            await _ctx.Invoices
                      .Include(i => i.Booking)
                      .Include(i => i.Payments)
                      .FirstOrDefaultAsync(i => i.Id == id);

        public async Task<IEnumerable<Invoice>> ListAsync() =>
            await _ctx.Invoices.ToListAsync();

        public async Task AddAsync(Invoice invoice) =>
            await _ctx.Invoices.AddAsync(invoice);

        public void Update(Invoice invoice) =>
            _ctx.Invoices.Update(invoice);

        public void Delete(Invoice invoice) =>
            _ctx.Invoices.Remove(invoice);

        public Task SaveChangesAsync() =>
            _ctx.SaveChangesAsync();
    }
}
