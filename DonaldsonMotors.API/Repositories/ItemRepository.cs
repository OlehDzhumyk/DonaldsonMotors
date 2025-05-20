using DonaldsonMotors.API.Data;
using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DonaldsonMotors.API.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly AppDbContext _ctx;
        public ItemRepository(AppDbContext ctx) => _ctx = ctx;

        public async Task<ItemEntity?> GetByIdAsync(int id) =>
            await _ctx.Items
                .Include(i => i.Supplier)
                .Include(i => i.JobItems)
                .FirstOrDefaultAsync(i => i.Id == id);

        public async Task<IEnumerable<ItemEntity>> ListAsync() =>
            await _ctx.Items.ToListAsync();

        public async Task AddAsync(ItemEntity itemEntity) =>
            await _ctx.Items.AddAsync(itemEntity);

        public void Update(ItemEntity itemEntity) =>
            _ctx.Items.Update(itemEntity);

        public void Delete(ItemEntity itemEntity) =>
            _ctx.Items.Remove(itemEntity);

        public Task SaveChangesAsync() =>
            _ctx.SaveChangesAsync();
    }
}