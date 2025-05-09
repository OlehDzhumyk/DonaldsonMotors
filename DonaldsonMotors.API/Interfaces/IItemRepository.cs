using DonaldsonMotors.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DonaldsonMotors.API.Interfaces
{
    public interface IItemRepository
    {
        Task<Item?> GetByIdAsync(int id);
        Task<IEnumerable<Item>> ListAsync();
        Task AddAsync(Item item);
        void Update(Item item);
        void Delete(Item item);
        Task SaveChangesAsync();
    }
}
