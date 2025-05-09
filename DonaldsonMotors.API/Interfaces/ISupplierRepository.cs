using DonaldsonMotors.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DonaldsonMotors.API.Interfaces
{
    public interface ISupplierRepository
    {
        Task<Supplier?> GetByIdAsync(int id);
        Task<IEnumerable<Supplier>> ListAsync();
        Task AddAsync(Supplier supplier);
        void Update(Supplier supplier);
        void Delete(Supplier supplier);
        Task SaveChangesAsync();
    }
}
