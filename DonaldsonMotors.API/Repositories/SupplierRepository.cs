using DonaldsonMotors.API.Data;
using DonaldsonMotors.API.Data.Entities;
using Microsoft.EntityFrameworkCore;
using DonaldsonMotors.API.Interfaces.Repositories;

namespace DonaldsonMotors.API.Repositories
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly AppDbContext _ctx;
        public SupplierRepository(AppDbContext ctx) => _ctx = ctx;

        public async Task<SupplierEntity?> GetByIdAsync(int id) =>
            await _ctx.Suppliers
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.Id == id);

        public async Task<IEnumerable<SupplierEntity>> ListAsync() =>
            await _ctx.Suppliers.ToListAsync();

        public async Task AddAsync(SupplierEntity supplierEntity) =>
            await _ctx.Suppliers.AddAsync(supplierEntity);

        public void Update(SupplierEntity supplierEntity) =>
            _ctx.Suppliers.Update(supplierEntity);

        public void Delete(SupplierEntity supplierEntity) =>
            _ctx.Suppliers.Remove(supplierEntity);

        public async Task SaveChangesAsync() =>
            await _ctx.SaveChangesAsync();
    }
}