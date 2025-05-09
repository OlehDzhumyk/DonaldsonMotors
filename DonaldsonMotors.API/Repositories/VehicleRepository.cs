using DonaldsonMotors.API.Data;
using DonaldsonMotors.API.Interfaces.Repositories;
using DonaldsonMotors.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DonaldsonMotors.API.Repositories
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly AppDbContext _ctx;
        public VehicleRepository(AppDbContext ctx) => _ctx = ctx;

        public async Task<Vehicle?> GetByIdAsync(string registration) =>
            await _ctx.Vehicles
                      .Include(v => v.Bookings)
                      .FirstOrDefaultAsync(v => v.RegistrationNumber == registration);

        public async Task<IEnumerable<Vehicle>> ListAsync() =>
            await _ctx.Vehicles.ToListAsync();

        public async Task AddAsync(Vehicle vehicle) =>
            await _ctx.Vehicles.AddAsync(vehicle);

        public void Update(Vehicle vehicle) =>
            _ctx.Vehicles.Update(vehicle);

        public void Delete(Vehicle vehicle) =>
            _ctx.Vehicles.Remove(vehicle);

        public Task SaveChangesAsync() =>
            _ctx.SaveChangesAsync();
    }
}
