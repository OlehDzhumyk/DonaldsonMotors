using DonaldsonMotors.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DonaldsonMotors.API.Interfaces
{
    public interface IVehicleRepository
    {
        Task<Vehicle?> GetByIdAsync(string registration);
        Task<IEnumerable<Vehicle>> ListAsync();
        Task AddAsync(Vehicle vehicle);
        void Update(Vehicle vehicle);
        void Delete(Vehicle vehicle);
        Task SaveChangesAsync();
    }
}
