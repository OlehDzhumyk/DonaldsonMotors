using DonaldsonMotors.API.Data;
using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DonaldsonMotors.API.Repositories
{
    /// <summary>
    /// Handles Vehicle data access. It's separate because its primary key is a string.
    /// </summary>
    public class VehicleRepository : IVehicleRepository
    {
        private readonly AppDbContext _context;
        public VehicleRepository(AppDbContext context) => _context = context;

        public async Task<Vehicle?> GetByRegistrationAsync(string registration) =>
            await _context.Vehicles.FirstOrDefaultAsync(v => v.RegistrationNumber == registration);

        public async Task<IEnumerable<Vehicle>> GetByOwnerIdAsync(int ownerId) =>
            await _context.Vehicles
                .Where(v => v.OwnerId == ownerId)
                .ToListAsync();

        public async Task AddAsync(Vehicle vehicle) =>
            await _context.Vehicles.AddAsync(vehicle);

        public void Update(Vehicle vehicle) =>
            _context.Vehicles.Update(vehicle);

        public void Delete(Vehicle vehicle) =>
            _context.Vehicles.Remove(vehicle);
    }
}