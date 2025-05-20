using DonaldsonMotors.API.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DonaldsonMotors.API.Interfaces.Repositories
{
    public interface IItemRepository
    {
        Task<ItemEntity?> GetByIdAsync(int id);
        Task<IEnumerable<ItemEntity>> ListAsync();
        Task AddAsync(ItemEntity itemEntity);
        void Update(ItemEntity itemEntity);
        void Delete(ItemEntity itemEntity);
        Task SaveChangesAsync();
    }
}