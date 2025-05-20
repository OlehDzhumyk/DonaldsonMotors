using DonaldsonMotors.API.Data.Entities;

namespace DonaldsonMotors.API.Interfaces.Repositories
{
    public interface IVehicleRepository
    {
        Task<VehicleEntity?> GetByIdAsync(string registration);
        Task<IEnumerable<VehicleEntity>> ListAsync();
        Task AddAsync(VehicleEntity vehicleEntity);
        void Update(VehicleEntity vehicleEntity);
        void Delete(VehicleEntity vehicleEntity);
        Task SaveChangesAsync();
    }
}