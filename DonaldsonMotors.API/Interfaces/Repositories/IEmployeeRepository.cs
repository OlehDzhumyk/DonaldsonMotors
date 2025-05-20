// Interfaces/Repositories/IEmployeeRepository.cs
using DonaldsonMotors.API.Domain.Models;

namespace DonaldsonMotors.API.Interfaces.Repositories
{
    public interface IEmployeeRepository
    {
        Task<Employee?> GetByIdAsync(int id);
        Task<IEnumerable<Employee>> ListAsync();
        Task AddAsync(Employee employee);
        void Update(Employee employee);
        void Delete(Employee employee);
        Task SaveChangesAsync();
    }
}
