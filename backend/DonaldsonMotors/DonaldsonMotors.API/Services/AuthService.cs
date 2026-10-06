using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.DTOs.Auth;
using DonaldsonMotors.API.Interfaces;
using DonaldsonMotors.API.Mappers;
using DonaldsonMotors.API.Exceptions;
using Microsoft.AspNetCore.Identity;
using static DonaldsonMotors.API.Exceptions.UserAlreadyExistsException;

namespace DonaldsonMotors.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IJwtService jwtService,
            IEmailService emailService,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
        {
            _logger.LogInformation("Registration attempt for {Email} with role {Role}", dto.Email, dto.Role);

            if (dto.Role != Roles.Customer &&
                dto.Role != Roles.Manager &&
                dto.Role != Roles.Mechanic &&
                dto.Role != Roles.StockController &&
                dto.Role != Roles.AccountsClerk)
            {
                // This check is more robust if the controller doesn't strictly control the role for staff
                _logger.LogWarning("Invalid role '{Role}' provided for registration of {Email}.", dto.Role, dto.Email);
                throw new InvalidRoleException(dto.Role, $"Invalid role '{dto.Role}' specified for registration.");
            }


            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("User with email {Email} already exists.", dto.Email);
                throw new UserAlreadyExistsException(dto.Email);
            }

            var userEntity = dto.ToApplicationUserEntity();

            var identityResult = await _userManager.CreateAsync(userEntity, dto.Password);
            if (!identityResult.Succeeded)
            {
                var errors = identityResult.Errors.Select(e => e.Description).ToList();
                _logger.LogWarning("User creation failed for {Email}: {Errors}", dto.Email, string.Join(", ", errors));
                // Throw custom exception for validation errors (e.g., password policy)
                throw new RegistrationValidationException(errors);
            }

            _logger.LogInformation("User {Email} created successfully with ID {UserId}.", userEntity.Email, userEntity.Id);

            // Ensure the role exists before assigning it
            if (!await _roleManager.RoleExistsAsync(dto.Role))
            {
                _logger.LogInformation("Role '{Role}' does not exist. Creating it.", dto.Role);
                var roleResult = await _roleManager.CreateAsync(new ApplicationRole { Name = dto.Role, NormalizedName = dto.Role.ToUpper() });
                if (!roleResult.Succeeded)
                {
                    var roleErrors = roleResult.Errors.Select(e => e.Description).ToList();
                    _logger.LogError("Failed to create role {Role}: {Errors}", dto.Role, string.Join(", ", roleErrors));
                    throw new AuthServiceException($"Failed to create role '{dto.Role}'.");
                }
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(userEntity, dto.Role);
            if (!addToRoleResult.Succeeded)
            {
                var roleAssignmentErrors = addToRoleResult.Errors.Select(e => e.Description).ToList();
                _logger.LogError("Failed to assign role {Role} to user {Email}: {Errors}", dto.Role, userEntity.Email, string.Join(", ", roleAssignmentErrors));
                // Potentially clean up created user or log for manual intervention
                throw new AuthServiceException($"Failed to assign role '{dto.Role}' to user.");
            }
            _logger.LogInformation("Assigned role '{Role}' to user {Email}", dto.Role, userEntity.Email);

            // Generate token for the new user
            var (token, expiresAt) = await _jwtService.GenerateTokenAsync(userEntity);
            _logger.LogInformation("Token generated for user {Email}", userEntity.Email);

            // Send welcome email
            await _emailService.SendWelcomeEmailAsync(userEntity.Email!, userEntity.FullName);
            _logger.LogInformation("Welcome email dispatch initiated for {Email}", userEntity.Email);

            return new AuthResponseDto
            {
                Email = userEntity.Email!,
                Role = dto.Role, // Role from the DTO as it was used for assignment
                Token = token,
                ExpiresAt = expiresAt
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
        {
            _logger.LogInformation("Login attempt for {Email}", dto.Email);

            var userEntity = await _userManager.FindByEmailAsync(dto.Email);
            if (userEntity == null)
            {
                _logger.LogWarning("Login failed: User with email {Email} not found.", dto.Email);
                // Throw custom exception for invalid credentials
                throw new InvalidCredentialsException();
            }

            if (!await _userManager.CheckPasswordAsync(userEntity, dto.Password))
            {
                _logger.LogWarning("Login failed: Invalid password for user {Email}", dto.Email);
                // Throw custom exception for invalid credentials
                throw new InvalidCredentialsException();
            }

            _logger.LogInformation("Authentication successful for {Email}", dto.Email);
            var (token, expiresAt) = await _jwtService.GenerateTokenAsync(userEntity);
            var roles = await _userManager.GetRolesAsync(userEntity);

            return new AuthResponseDto
            {
                Email = userEntity.Email!,
                Role = roles.FirstOrDefault() ?? string.Empty, // Get the actual role from user
                Token = token,
                ExpiresAt = expiresAt
            };
        }
    }
}