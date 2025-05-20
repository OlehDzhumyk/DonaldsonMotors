using DonaldsonMotors.API.Data;
using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DonaldsonMotors.API.Repositories
{
    public class JobRepository : IJobRepository
    {
        private readonly AppDbContext _ctx;
        public JobRepository(AppDbContext ctx) => _ctx = ctx;

        public async Task<JobEntity?> GetByIdAsync(int id) =>
            await _ctx.Jobs
                .Include(j => j.Technician) // Assuming Technician is a CustomerEntity for now
                .Include(j => j.Booking)
                .Include(j => j.Items)
                    .ThenInclude(ji => ji.Item)
                .FirstOrDefaultAsync(j => j.Id == id);

        public async Task<IEnumerable<JobEntity>> ListAsync() =>
            await _ctx.Jobs.ToListAsync();

        public async Task AddAsync(JobEntity jobEntity) =>
            await _ctx.Jobs.AddAsync(jobEntity);

        public void Update(JobEntity jobEntity) =>
            _ctx.Jobs.Update(jobEntity);

        public void Delete(JobEntity jobEntity) =>
            _ctx.Jobs.Remove(jobEntity);

        public Task SaveChangesAsync() =>
            _ctx.SaveChangesAsync();
    }
}