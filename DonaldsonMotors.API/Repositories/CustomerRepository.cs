using DonaldsonMotors.API.Data;
using DonaldsonMotors.API.Interfaces.Repositories;
using DonaldsonMotors.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DonaldsonMotors.API.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _ctx;
        public CustomerRepository(AppDbContext ctx) => _ctx = ctx;

        public async Task<Customer?> GetByIdAsync(string id) =>
            await _ctx.Users
                      .OfType<Customer>()
                      .Include(c => c.Vehicles)
                      .Include(c => c.Bookings)
                      .FirstOrDefaultAsync(c => c.Id.ToString() == id);

        public async Task<Customer?> GetByEmailAsync(string email) =>
            await _ctx.Users
                      .OfType<Customer>()
                      .FirstOrDefaultAsync(c => c.Email == email);

        public async Task AddAsync(Customer customer) =>
            await _ctx.Users.AddAsync(customer);

        public void Update(Customer customer) =>
            _ctx.Users.Update(customer);

        public void Delete(Customer customer) =>
            _ctx.Users.Remove(customer);

        public Task SaveChangesAsync() =>
            _ctx.SaveChangesAsync();
    }
}
