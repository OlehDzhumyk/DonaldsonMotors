using DonaldsonMotors.API.Domain.Models;
using DonaldsonMotors.API.Interfaces.Repositories;
using DonaldsonMotors.API.Interfaces.Services;
using DonaldsonMotors.API.Mappers;
using Microsoft.Extensions.Logging;

public class UserService : IUserService
{
    private readonly ICustomerRepository _customers;
    private readonly IVehicleRepository _vehicles;
    private readonly ILogger<UserService> _logger;

    public UserService(ICustomerRepository customers, IVehicleRepository vehicles, ILogger<UserService> logger)
    {
        _customers = customers;
        _vehicles = vehicles;
        _logger = logger;
    }

    public async Task<Customer?> GetProfileAsync(int userId)
    {
        _logger.LogDebug("Getting customer profile for user ID {UserId}", userId);
        var customer = await _customers.GetByIdAsync(userId);
        return customer?.ToDomainModel();
    }

    public async Task<bool> UpdateProfileAsync(int userId, string fullName, string? address, string? telephone)
    {
        var customer = await _customers.GetByIdAsync(userId);
        if (customer is null)
        {
            _logger.LogWarning("UpdateProfileAsync: Customer not found for user ID {UserId}", userId);
            return false;
        }

        customer.UserName = fullName;
        customer.Address = address;
        customer.PhoneNumber = telephone;

        _customers.Update(customer);
        await _customers.SaveChangesAsync();

        _logger.LogInformation("Profile updated for user ID {UserId}", userId);
        return true;
    }

    public async Task<Vehicle> AddVehicleAsync(int userId, Vehicle vehicle)
    {
        _logger.LogInformation("Adding vehicle {Reg} for user {UserId}", vehicle.Registration, userId);
        var entity = vehicle.ToEntity(userId);
        await _vehicles.AddAsync(entity);
        await _vehicles.SaveChangesAsync();
        return entity.ToDomainModel();
    }

    public async Task<bool> UpdateVehicleAsync(int userId, string reg, Vehicle vehicle)
    {
        var existing = await _vehicles.GetByIdAsync(reg);
        if (existing == null || existing.CustomerId != userId)
        {
            _logger.LogWarning("UpdateVehicleAsync: Vehicle {Reg} not found or does not belong to user {UserId}", reg, userId);
            return false;
        }

        existing.Make = vehicle.Make;
        existing.Model = vehicle.Model;
        existing.Year = vehicle.Year;
        existing.Mileage = vehicle.Mileage;

        _vehicles.Update(existing);
        await _vehicles.SaveChangesAsync();

        _logger.LogInformation("Vehicle {Reg} updated for user {UserId}", reg, userId);
        return true;
    }

    public async Task<bool> DeleteVehicleAsync(int userId, string reg)
    {
        var existing = await _vehicles.GetByIdAsync(reg);
        if (existing == null || existing.CustomerId != userId)
        {
            _logger.LogWarning("DeleteVehicleAsync: Vehicle {Reg} not found or does not belong to user {UserId}", reg, userId);
            return false;
        }

        _vehicles.Delete(existing);
        await _vehicles.SaveChangesAsync();

        _logger.LogInformation("Vehicle {Reg} deleted for user {UserId}", reg, userId);
        return true;
    }
}
