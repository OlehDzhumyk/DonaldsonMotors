using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.DTOs.User; 
using DonaldsonMotors.API.DTOs.Vehicle;
using DonaldsonMotors.API.Interfaces;
using DonaldsonMotors.API.Mappers;
using DonaldsonMotors.API.Exceptions;
using Microsoft.AspNetCore.Identity;


namespace DonaldsonMotors.API.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserService> _logger;

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IUnitOfWork unitOfWork,
            ILogger<UserService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<UserProfileResponseDto> GetProfileAsync(int userId)
        {
            _logger.LogInformation("Fetching profile for UserId {UserId}", userId);
            var customer = await _unitOfWork.Users.GetCustomerByIdWithVehiclesAsync(userId)
                ?? throw new UserProfileNotFoundException(userId);

            return customer.ToProfileResponseDto();
        }

        public async Task<UserProfileResponseDto> UpdateProfileAsync(int userId, UpdateProfileRequestDto dto)
        {
            _logger.LogInformation("Updating profile for UserId {UserId}", userId);
            var user = await _unitOfWork.Users.GetUserByIdAsync(userId)
                ?? throw new UserProfileNotFoundException(userId);

            user.FullName = dto.FullName ?? user.FullName;
            user.Address = dto.Address ?? user.Address;
            user.PhoneNumber = dto.TelephoneNumber ?? user.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                _logger.LogError("Failed to update profile for UserId {UserId}: {Errors}", userId, string.Join(", ", errors));
                throw new ProfileUpdateFailedException(errors);
            }

            return await GetProfileAsync(userId);
        }

        public async Task<VehicleResponseDto> AddVehicleAsync(int userId, VehicleRequestDto dto)
        {
            _logger.LogInformation("User {UserId} adding vehicle {Registration}", userId, dto.RegistrationNumber);

            if (await _unitOfWork.Vehicles.GetByRegistrationAsync(dto.RegistrationNumber) != null)
                throw new VehicleAlreadyExistsException(dto.RegistrationNumber);

            var owner = await _unitOfWork.Users.GetUserByIdAsync(userId);
            if (owner is not Customer)
                throw new UserProfileNotFoundException(userId, $"A customer with ID {userId} must exist to add a vehicle.");

            var vehicleEntity = dto.ToVehicleEntity(userId);
            await _unitOfWork.Vehicles.AddAsync(vehicleEntity);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Vehicle {Registration} added for UserId {UserId}", dto.RegistrationNumber, userId);
            return vehicleEntity.ToVehicleResponseDto();
        }

        public async Task UpdateVehicleAsync(int userId, string registration, VehicleRequestDto dto)
        {
            _logger.LogInformation("User {UserId} updating vehicle {Registration}", userId, registration);
            var vehicle = await _unitOfWork.Vehicles.GetByRegistrationAsync(registration)
                ?? throw new VehicleNotFoundException(registration);
            if (vehicle.OwnerId != userId)
                throw new VehicleAccessDeniedException(registration, userId);

            vehicle.UpdateVehicleEntity(dto);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Vehicle {Registration} updated for UserId {UserId}", registration, userId);
        }

        public async Task DeleteVehicleAsync(int userId, string registration)
        {
            _logger.LogInformation("User {UserId} deleting vehicle {Registration}", userId, registration);
            var vehicle = await _unitOfWork.Vehicles.GetByRegistrationAsync(registration)
                ?? throw new VehicleNotFoundException(registration);
            if (vehicle.OwnerId != userId)
                throw new VehicleAccessDeniedException(registration, userId);

            var activeBookings = await _unitOfWork.Bookings.FindAsync(b =>
                b.VehicleRegistrationNumber == registration &&
                b.Status != BookingStatus.Cancelled &&
                b.Status != BookingStatus.Paid &&
                b.Status != BookingStatus.Archived);
            if (activeBookings.Any())
            {
                throw new InvalidOperationException($"Cannot delete vehicle '{registration}' as it has active or pending bookings.");
            }
            _unitOfWork.Vehicles.Delete(vehicle);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Vehicle {Registration} deleted for UserId {UserId}", registration, userId);
        }

        // --- NEW METHODS IMPLEMENTATION ---

        public async Task<UserProfileResponseDto> AdminUpdateUserAsync(int userIdToEdit, AdminUpdateUserRequestDto dto)
        {
            _logger.LogInformation("Admin attempting to update profile for UserId {UserIdToEdit}", userIdToEdit);
            var userToEdit = await _userManager.FindByIdAsync(userIdToEdit.ToString())
                ?? throw new UserProfileNotFoundException(userIdToEdit);

            // Update basic properties
            userToEdit.FullName = dto.FullName ?? userToEdit.FullName;
            userToEdit.Address = dto.Address ?? userToEdit.Address;
            userToEdit.PhoneNumber = dto.TelephoneNumber ?? userToEdit.PhoneNumber;

            // Update Email if provided and changed
            if (!string.IsNullOrWhiteSpace(dto.Email) && !userToEdit.Email.Equals(dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                // Check if the new email is already taken
                var existingUserWithNewEmail = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUserWithNewEmail != null && existingUserWithNewEmail.Id != userToEdit.Id)
                {
                    throw new UserAlreadyExistsException(dto.Email, $"Cannot update to email '{dto.Email}' as it's already in use by another user.");
                }
                var setEmailResult = await _userManager.SetEmailAsync(userToEdit, dto.Email);
                if (!setEmailResult.Succeeded) throw new ProfileUpdateFailedException(setEmailResult.Errors.Select(e => e.Description));

                var setUserNameResult = await _userManager.SetUserNameAsync(userToEdit, dto.Email); // Usually UserName is Email
                if (!setUserNameResult.Succeeded) throw new ProfileUpdateFailedException(setUserNameResult.Errors.Select(e => e.Description));
            }

            // Update EmailConfirmed status if provided
            if (dto.IsEmailConfirmed.HasValue && userToEdit.EmailConfirmed != dto.IsEmailConfirmed.Value)
            {
                userToEdit.EmailConfirmed = dto.IsEmailConfirmed.Value;
            }

            var updateResult = await _userManager.UpdateAsync(userToEdit);
            if (!updateResult.Succeeded)
            {
                throw new ProfileUpdateFailedException(updateResult.Errors.Select(e => e.Description));
            }

            if (userToEdit is Employee && !string.IsNullOrWhiteSpace(dto.Role))
            {
                var currentRoles = await _userManager.GetRolesAsync(userToEdit);
                if (!currentRoles.Contains(dto.Role)) // If role is actually different
                {
                    if (!await _roleManager.RoleExistsAsync(dto.Role))
                    {
                        throw new InvalidRoleException(dto.Role, $"Role '{dto.Role}' does not exist.");
                    }
                    var removeRolesResult = await _userManager.RemoveFromRolesAsync(userToEdit, currentRoles);
                    if (!removeRolesResult.Succeeded) throw new ProfileUpdateFailedException(removeRolesResult.Errors.Select(e => e.Description));

                    var addRoleResult = await _userManager.AddToRoleAsync(userToEdit, dto.Role);
                    if (!addRoleResult.Succeeded) throw new ProfileUpdateFailedException(addRoleResult.Errors.Select(e => e.Description));
                    _logger.LogInformation("Admin updated role for UserId {UserIdToEdit} to {NewRole}", userIdToEdit, dto.Role);
                }
            }

            _logger.LogInformation("Admin successfully updated profile for UserId {UserIdToEdit}", userIdToEdit);

            var updatedUser = await _unitOfWork.Users.GetUserByIdAsync(userIdToEdit);

            if (updatedUser is Customer updatedCustomer)
            {
                // Get the full profile with vehicles for customers.
                return (await _unitOfWork.Users.GetCustomerByIdWithVehiclesAsync(updatedCustomer.Id))
                    .ToProfileResponseDto();
            }

            // For non-customer users (Employees), return a basic profile DTO.
            return new UserProfileResponseDto
            {
                Id = updatedUser.Id,
                FullName = updatedUser.FullName,
                Email = updatedUser.Email ?? "",
                PhoneNumber = updatedUser.PhoneNumber,
                Address = updatedUser.Address
            };
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordRequestDto dto)
        {
            _logger.LogInformation("User {UserId} attempting to change password.", userId);
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new UserProfileNotFoundException(userId);

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                _logger.LogWarning("Password change failed for UserId {UserId}: {Errors}", userId, string.Join(", ", errors));
                throw new ProfileUpdateFailedException("Password change failed.", errors);
            }

            _logger.LogInformation("Password changed successfully for UserId {UserId}", userId);
        }
    }
}