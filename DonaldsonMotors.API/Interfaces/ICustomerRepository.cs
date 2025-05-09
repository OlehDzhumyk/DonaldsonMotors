using DonaldsonMotors.API.Models;
using System.Threading.Tasks;

namespace DonaldsonMotors.API.Interfaces
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(string id);
        Task<Customer?> GetByEmailAsync(string email);
        Task AddAsync(Customer customer);
        void Update(Customer customer);
        void Delete(Customer customer);
        Task SaveChangesAsync();
    }
}
