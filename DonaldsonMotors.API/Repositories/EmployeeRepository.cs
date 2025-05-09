using DonaldsonMotors.API.Data;
using DonaldsonMotors.API.Interfaces.Repositories;
using DonaldsonMotors.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DonaldsonMotors.API.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _ctx;
        public EmployeeRepository(AppDbContext ctx) => _ctx = ctx;

        public async Task<Employee?> GetByIdAsync(int id) =>
            await _ctx.Users
                      .OfType<Employee>()
                      .Include(e => e.JobsCompleted)
                      .FirstOrDefaultAsync(e => e.Id == id);

        public async Task AddAsync(Employee employee) =>
            await _ctx.Users.AddAsync(employee);

        public void Update(Employee employee) =>
            _ctx.Users.Update(employee);

        public void Delete(Employee employee) =>
            _ctx.Users.Remove(employee);

        public Task SaveChangesAsync() =>
            _ctx.SaveChangesAsync();
    }
}
