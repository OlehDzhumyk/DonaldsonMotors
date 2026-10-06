using DonaldsonMotors.API.Data;
using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DonaldsonMotors.API.Repositories
{
    public class PartRepository : GenericRepository<Part>, IPartRepository
    {
        public PartRepository(AppDbContext context) : base(context) { }

        public async Task<Part?> GetByIdWithSupplierAsync(int id)
        {
            return await _context.Parts
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Part>> GetAllWithSupplierAsync()
        {
            return await _context.Parts
                .Include(p => p.Supplier)
                .ToListAsync();
        }
    }
}