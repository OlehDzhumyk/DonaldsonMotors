using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Data;
using Microsoft.EntityFrameworkCore;

public class VehicleRepository : IVehicleRepository
{
    private readonly AppDbContext _ctx;
    public VehicleRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<VehicleEntity?> GetByIdAsync(string registration) =>
        await _ctx.Vehicles.FirstOrDefaultAsync(v => v.RegistrationNumber == registration);

    public async Task<IEnumerable<VehicleEntity>> ListAsync() =>
        await _ctx.Vehicles.ToListAsync();

    public async Task<IEnumerable<VehicleEntity>> GetByCustomerIdAsync(int customerId) => // ⬅️ NEW
        await _ctx.Vehicles
            .Where(v => v.CustomerId == customerId)
            .ToListAsync();

    public async Task AddAsync(VehicleEntity vehicleEntity) =>
        await _ctx.Vehicles.AddAsync(vehicleEntity);

    public void Update(VehicleEntity vehicleEntity) =>
        _ctx.Vehicles.Update(vehicleEntity);

    public void Delete(VehicleEntity vehicleEntity) =>
        _ctx.Vehicles.Remove(vehicleEntity);

    public async Task SaveChangesAsync() =>
        await _ctx.SaveChangesAsync();
}
