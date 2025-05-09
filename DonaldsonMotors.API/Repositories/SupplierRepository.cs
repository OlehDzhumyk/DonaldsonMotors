using DonaldsonMotors.API.Data;
using DonaldsonMotors.API.Interfaces.Repositories;
using DonaldsonMotors.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DonaldsonMotors.API.Repositories
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly AppDbContext _ctx;
        public SupplierRepository(AppDbContext ctx) => _ctx = ctx;

        public async Task<Supplier?> GetByIdAsync(int id) =>
            await _ctx.Suppliers
                      .Include(s => s.Items)
                      .FirstOrDefaultAsync(s => s.Id == id);

        public async Task<IEnumerable<Supplier>> ListAsync() =>
            await _ctx.Suppliers.ToListAsync();

        public async Task AddAsync(Supplier supplier) =>
            await _ctx.Suppliers.AddAsync(supplier);

        public void Update(Supplier supplier) =>
            _ctx.Suppliers.Update(supplier);

        public void Delete(Supplier supplier) =>
            _ctx.Suppliers.Remove(supplier);

        public Task SaveChangesAsync() =>
            _ctx.SaveChangesAsync();
    }
}
