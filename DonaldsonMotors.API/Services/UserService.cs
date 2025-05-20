using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Domain.Models;
using DonaldsonMotors.API.Interfaces.Repositories;
using DonaldsonMotors.API.Interfaces.Services;
using DonaldsonMotors.API.Mappers;
using System.Threading.Tasks;

namespace DonaldsonMotors.API.Services
{
    public class UserService : IUserService
    {
        private readonly ICustomerRepository _customers;
        private readonly IVehicleRepository _vehicles;

        public UserService(
            ICustomerRepository customers,
            IVehicleRepository vehicles)
        {
            _customers = customers;
            _vehicles = vehicles;
        }

        public async Task<Customer?> GetProfileAsync(int userId)
        {
            var customerEntity = await _customers.GetByIdAsync(userId);
            return customerEntity?.ToDomainModel();
        }

        public async Task<bool> UpdateProfileAsync(int userId, string fullName, string? address, string? telephone)
        {
            var customerEntity = await _customers.GetByIdAsync(userId);
            if (customerEntity == null) return false;

            customerEntity.UserName = fullName; // Assuming UserName maps to FullName in Identity
            customerEntity.Address = address;
            customerEntity.PhoneNumber = telephone; // Assuming PhoneNumber maps to Telephone

            _customers.Update(customerEntity);
            await _customers.SaveChangesAsync();
            return true;
        }

        public async Task<Vehicle> AddVehicleAsync(int userId, Vehicle vehicle)
        {
            var vehicleEntity = vehicle.ToEntity(userId); // Use the ToEntity extension, providing the ownerId
            await _vehicles.AddAsync(vehicleEntity);
            await _vehicles.SaveChangesAsync();
            return vehicleEntity.ToDomainModel(); // Convert the saved entity back to the domain model
        }

        public async Task<bool> UpdateVehicleAsync(int userId, string registration, Vehicle vehicle)
        {
            var existingEntity = await _vehicles.GetByIdAsync(registration);
            if (existingEntity == null || existingEntity.CustomerId != userId) return false;

            existingEntity.Make = vehicle.Make;
            existingEntity.Model = vehicle.Model;
            existingEntity.Year = vehicle.Year;
            existingEntity.Mileage = vehicle.Mileage;

            _vehicles.Update(existingEntity);
            await _vehicles.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteVehicleAsync(int userId, string registration)
        {
            var existingEntity = await _vehicles.GetByIdAsync(registration);
            if (existingEntity == null || existingEntity.CustomerId != userId) return false;

            _vehicles.Delete(existingEntity);
            await _vehicles.SaveChangesAsync();
            return true;
        }
    }
}