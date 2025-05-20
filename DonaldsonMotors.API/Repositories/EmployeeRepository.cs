// Repositories/EmployeeRepository.cs
using DonaldsonMotors.API.Data;
using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Domain.Models;
using DonaldsonMotors.API.Interfaces.Repositories;
using DonaldsonMotors.API.Mappers;
using Microsoft.EntityFrameworkCore;

namespace DonaldsonMotors.API.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _ctx;

        public EmployeeRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<Employee?> GetByIdAsync(int id)
        {
            var entity = await _ctx.Set<EmployeeEntity>()
                                   .AsNoTracking()
                                   .FirstOrDefaultAsync(e => e.Id == id);
            return entity?.ToDomainModel();
        }

        public async Task<IEnumerable<Employee>> ListAsync()
        {
            var entities = await _ctx.Set<EmployeeEntity>()
                                     .AsNoTracking()
                                     .ToListAsync();
            return entities.Select(e => e.ToDomainModel());
        }

        public async Task AddAsync(Employee employee)
        {
            var entity = employee.ToEntity();
            await _ctx.Set<EmployeeEntity>().AddAsync(entity);
            // note: identity user creation often goes through UserManager, not direct EF
        }

        public void Update(Employee employee)
        {
            var entity = employee.ToEntity();
            _ctx.Set<EmployeeEntity>().Update(entity);
        }

        public void Delete(Employee employee)
        {
            var entity = employee.ToEntity();
            _ctx.Set<EmployeeEntity>().Remove(entity);
        }

        public Task SaveChangesAsync()
            => _ctx.SaveChangesAsync();
    }
}
