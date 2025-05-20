using DonaldsonMotors.API.Data.Entities;

namespace DonaldsonMotors.API.Interfaces.Repositories
{
    public interface ICustomerRepository
    {
        Task<CustomerEntity?> GetByIdAsync(int id);
        Task<CustomerEntity?> GetByEmailAsync(string email);
        Task<IEnumerable<CustomerEntity>> ListAsync();
        Task AddAsync(CustomerEntity customerEntity);
        void Update(CustomerEntity customerEntity);
        void Delete(CustomerEntity customerEntity);
        Task SaveChangesAsync();
    }
}