using DonaldsonMotors.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DonaldsonMotors.API.Interfaces.Repositories
{
    public interface IJobRepository
    {
        Task<Job?> GetByIdAsync(int id);
        Task<IEnumerable<Job>> ListAsync();
        Task AddAsync(Job job);
        void Update(Job job);
        void Delete(Job job);
        Task SaveChangesAsync();
    }
}
