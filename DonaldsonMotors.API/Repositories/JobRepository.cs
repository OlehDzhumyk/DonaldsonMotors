using DonaldsonMotors.API.Data;
using DonaldsonMotors.API.Interfaces;
using DonaldsonMotors.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DonaldsonMotors.API.Repositories
{
    public class JobRepository : IJobRepository
    {
        private readonly AppDbContext _ctx;
        public JobRepository(AppDbContext ctx) => _ctx = ctx;

        public async Task<Job?> GetByIdAsync(int id) =>
            await _ctx.Jobs
                      .Include(j => j.Technician)
                      .Include(j => j.Booking)
                      .Include(j => j.JobItems)
                          .ThenInclude(ji => ji.Item)
                      .FirstOrDefaultAsync(j => j.Id == id);

        public async Task<IEnumerable<Job>> ListAsync() =>
            await _ctx.Jobs.ToListAsync();

        public async Task AddAsync(Job job) =>
            await _ctx.Jobs.AddAsync(job);

        public void Update(Job job) =>
            _ctx.Jobs.Update(job);

        public void Delete(Job job) =>
            _ctx.Jobs.Remove(job);

        public Task SaveChangesAsync() =>
            _ctx.SaveChangesAsync();
    }
}
