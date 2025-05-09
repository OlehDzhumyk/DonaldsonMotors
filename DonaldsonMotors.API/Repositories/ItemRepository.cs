using DonaldsonMotors.API.Data;
using DonaldsonMotors.API.Interfaces.Repositories;
using DonaldsonMotors.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DonaldsonMotors.API.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly AppDbContext _ctx;
        public ItemRepository(AppDbContext ctx) => _ctx = ctx;

        public async Task<Item?> GetByIdAsync(int id) =>
            await _ctx.Items
                      .Include(i => i.Supplier)
                      .Include(i => i.JobItems)
                      .FirstOrDefaultAsync(i => i.Id == id);

        public async Task<IEnumerable<Item>> ListAsync() =>
            await _ctx.Items.ToListAsync();

        public async Task AddAsync(Item item) =>
            await _ctx.Items.AddAsync(item);

        public void Update(Item item) =>
            _ctx.Items.Update(item);

        public void Delete(Item item) =>
            _ctx.Items.Remove(item);

        public Task SaveChangesAsync() =>
            _ctx.SaveChangesAsync();
    }
}
