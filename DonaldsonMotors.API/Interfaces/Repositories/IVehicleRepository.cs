using DonaldsonMotors.API.Data.Entities;

public interface IVehicleRepository
{
    Task<VehicleEntity?> GetByIdAsync(string registration);
    Task<IEnumerable<VehicleEntity>> ListAsync();
    Task<IEnumerable<VehicleEntity>> GetByCustomerIdAsync(int customerId); // ⬅️ NEW
    Task AddAsync(VehicleEntity vehicleEntity);
    void Update(VehicleEntity vehicleEntity);
    void Delete(VehicleEntity vehicleEntity);
    Task SaveChangesAsync();
}
