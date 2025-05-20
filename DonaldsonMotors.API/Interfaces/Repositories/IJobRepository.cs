using DonaldsonMotors.API.Data.Entities;

namespace DonaldsonMotors.API.Interfaces.Repositories
{
    public interface IJobRepository
    {
        Task<JobEntity?> GetByIdAsync(int id);
        Task<IEnumerable<JobEntity>> ListAsync();
        Task AddAsync(JobEntity jobEntity);
        void Update(JobEntity jobEntity);
        void Delete(JobEntity jobEntity);
        Task SaveChangesAsync();
    }
}