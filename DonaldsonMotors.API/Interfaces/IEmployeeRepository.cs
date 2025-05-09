using DonaldsonMotors.API.Models;
using System.Threading.Tasks;

namespace DonaldsonMotors.API.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<Employee?> GetByIdAsync(int id);
        Task AddAsync(Employee employee);
        void Update(Employee employee);
        void Delete(Employee employee);
        Task SaveChangesAsync();
    }
}
