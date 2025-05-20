using DonaldsonMotors.API.Data;
using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DonaldsonMotors.API.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _ctx;
        public CustomerRepository(AppDbContext ctx) => _ctx = ctx;

        public async Task<CustomerEntity?> GetByIdAsync(int id) =>
            await _ctx.Users
                .OfType<CustomerEntity>()
                .Include(c => c.Vehicles)
                .Include(c => c.Bookings)
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<CustomerEntity?> GetByEmailAsync(string email) =>
            await _ctx.Users
                .OfType<CustomerEntity>()
                .FirstOrDefaultAsync(c => c.Email == email);

        public async Task AddAsync(CustomerEntity customerEntity) =>
            await _ctx.Users.AddAsync(customerEntity);

        public void Update(CustomerEntity customerEntity) =>
            _ctx.Users.Update(customerEntity);

        public void Delete(CustomerEntity customerEntity) =>
            _ctx.Users.Remove(customerEntity);

        public async Task<IEnumerable<CustomerEntity>> ListAsync() =>
            await _ctx.Users
                .OfType<CustomerEntity>()
                .ToListAsync();

        public Task SaveChangesAsync() =>
            _ctx.SaveChangesAsync();
    }
}