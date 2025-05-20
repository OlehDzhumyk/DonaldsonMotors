using DonaldsonMotors.API.Data;
using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DonaldsonMotors.API.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly AppDbContext _ctx;
        public InvoiceRepository(AppDbContext ctx) => _ctx = ctx;

        public async Task<InvoiceEntity?> GetByIdAsync(int id) =>
            await _ctx.Invoices
                .Include(i => i.Booking)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == id);

        public async Task<IEnumerable<InvoiceEntity>> ListAsync() =>
            await _ctx.Invoices.ToListAsync();

        public async Task AddAsync(InvoiceEntity invoiceEntity) =>
            await _ctx.Invoices.AddAsync(invoiceEntity);

        public void Update(InvoiceEntity invoiceEntity) =>
            _ctx.Invoices.Update(invoiceEntity);

        public void Delete(InvoiceEntity invoiceEntity) =>
            _ctx.Invoices.Remove(invoiceEntity);

        public async Task SaveChangesAsync() =>
            await _ctx.SaveChangesAsync();
    }
}