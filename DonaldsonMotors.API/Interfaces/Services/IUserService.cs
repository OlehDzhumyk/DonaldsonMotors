using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Domain.Models;

namespace DonaldsonMotors.API.Interfaces.Services
{
    public interface IUserService
    {
        Task<Customer?> GetProfileAsync(int userId);
        Task<bool> UpdateProfileAsync(int userId, string fullName, string? address, string? telephone);
        Task<Vehicle> AddVehicleAsync(int userId, Vehicle vehicle);
        Task<bool> UpdateVehicleAsync(int userId, string registration, Vehicle vehicle);
        Task<bool> DeleteVehicleAsync(int userId, string registration);
    }
}
