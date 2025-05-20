using DonaldsonMotors.API.Data.Entities;

namespace DonaldsonMotors.API.Interfaces.Repositories
{
    public interface ISupplierRepository
    {
        Task<SupplierEntity?> GetByIdAsync(int id);
        Task<IEnumerable<SupplierEntity>> ListAsync();
        Task AddAsync(SupplierEntity supplier);
        void Update(SupplierEntity supplier);
        void Delete(SupplierEntity supplier);
        Task SaveChangesAsync();
    }
}