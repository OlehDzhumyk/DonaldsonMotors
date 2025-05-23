using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.DTOs.User;
using DonaldsonMotors.API.DTOs.Vehicle;
using DonaldsonMotors.API.Interfaces;
using DonaldsonMotors.API.Mappers;
using Microsoft.AspNetCore.Identity;

namespace DonaldsonMotors.API.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserService> _logger;

        // The service now depends on IUnitOfWork, not individual repositories.
        public UserService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, ILogger<UserService> logger)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<UserProfileResponseDto> GetProfileAsync(int userId)
        {
            _logger.LogInformation("Fetching profile for UserId {UserId}", userId);

            // Use the repository to get the customer and their vehicles in one go.
            var customer = await _unitOfWork.Customers.GetByIdWithVehiclesAsync(userId)
                ?? throw new KeyNotFoundException($"Customer with ID {userId} not found");

            return customer.ToProfileResponseDto();
        }

        public async Task<UserProfileResponseDto> UpdateProfileAsync(int userId, UpdateProfileRequestDto dto)
        {
            _logger.LogInformation("Updating profile for UserId {UserId}", userId);

            // For Identity-managed properties, we must use UserManager.
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new KeyNotFoundException($"User with ID {userId} not found");

            user.FullName = dto.FullName ?? user.FullName;
            user.Address = dto.Address ?? user.Address;
            user.PhoneNumber = dto.TelephoneNumber ?? user.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                // Log the actual Identity errors
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to update profile for UserId {UserId}: {Errors}", userId, errors);
                throw new InvalidOperationException($"Failed to update profile: {errors}");
            }

            // After successful update, get the full profile to return.
            return await GetProfileAsync(userId);
        }

        public async Task<VehicleResponseDto> AddVehicleAsync(int userId, VehicleRequestDto dto)
        {
            _logger.LogInformation("User {UserId} adding vehicle {Registration}", userId, dto.RegistrationNumber);

            var existing = await _unitOfWork.Vehicles.GetByRegistrationAsync(dto.RegistrationNumber);
            if (existing != null)
                throw new InvalidOperationException($"Vehicle with registration '{dto.RegistrationNumber}' already exists.");

            var vehicle = dto.ToVehicleEntity(userId);

            // Use the Unit of Work to add the vehicle
            await _unitOfWork.Vehicles.AddAsync(vehicle);
            // Save all changes atomically
            await _unitOfWork.CompleteAsync();

            return vehicle.ToVehicleResponseDto();
        }

        public async Task UpdateVehicleAsync(int userId, string registration, VehicleRequestDto dto)
        {
            _logger.LogInformation("User {UserId} updating vehicle {Registration}", userId, registration);

            var vehicle = await _unitOfWork.Vehicles.GetByRegistrationAsync(registration)
                ?? throw new KeyNotFoundException($"Vehicle '{registration}' not found.");

            if (vehicle.OwnerId != userId)
                throw new UnauthorizedAccessException("You are not authorized to update this vehicle.");

            vehicle.UpdateVehicleEntity(dto);

            _unitOfWork.Vehicles.Update(vehicle);
            _unitOfWork.Vehicles.Update(vehicle);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteVehicleAsync(int userId, string registration)
        {
            _logger.LogInformation("User {UserId} deleting vehicle {Registration}", userId, registration);

            var vehicle = await _unitOfWork.Vehicles.GetByRegistrationAsync(registration)
                ?? throw new KeyNotFoundException($"Vehicle '{registration}' not found.");

            if (vehicle.OwnerId != userId)
                throw new UnauthorizedAccessException("You are not authorized to delete this vehicle.");

            _unitOfWork.Vehicles.Delete(vehicle);
            await _unitOfWork.CompleteAsync();
        }
    }
}