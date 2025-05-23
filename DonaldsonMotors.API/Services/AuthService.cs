using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.DTOs.Auth;
using DonaldsonMotors.API.Interfaces;
using DonaldsonMotors.API.Mappers;
using Microsoft.AspNetCore.Identity;

namespace DonaldsonMotors.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IJwtService jwtService,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
        {
            _logger.LogInformation("Registration attempt for {Email} with role {Role}", dto.Email, dto.Role);

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"User with email '{dto.Email}' already exists.");
            }

            var userEntity = dto.ToApplicationUserEntity(); // Using the mapper

            var identityResult = await _userManager.CreateAsync(userEntity, dto.Password);
            if (!identityResult.Succeeded)
            {
                var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                _logger.LogWarning("User creation failed for {Email}: {Errors}", dto.Email, errors);
                // Throw ArgumentException for validation issues like weak password
                throw new ArgumentException($"User creation failed: {errors}");
            }

            // Ensure the role exists before assigning it
            if (!await _roleManager.RoleExistsAsync(dto.Role))
            {
                _logger.LogInformation("Role '{Role}' does not exist. Creating it.", dto.Role);
                await _roleManager.CreateAsync(new ApplicationRole { Name = dto.Role });
            }

            await _userManager.AddToRoleAsync(userEntity, dto.Role);
            _logger.LogInformation("Assigned role '{Role}' to user {Email}", dto.Role, dto.Email);

            // Generate token for the new user
            var (token, expiresAt) = await _jwtService.GenerateTokenAsync(userEntity);

            return new AuthResponseDto
            {
                Email = userEntity.Email!,
                Role = dto.Role,
                Token = token,
                ExpiresAt = expiresAt
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
        {
            _logger.LogInformation("Login attempt for {Email}", dto.Email);

            var userEntity = await _userManager.FindByEmailAsync(dto.Email)
                ?? throw new UnauthorizedAccessException("Invalid email or password.");

            if (!await _userManager.CheckPasswordAsync(userEntity, dto.Password))
            {
                _logger.LogWarning("Invalid password for user {Email}", dto.Email);
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            _logger.LogInformation("Authentication successful for {Email}", dto.Email);
            var (token, expiresAt) = await _jwtService.GenerateTokenAsync(userEntity);
            var roles = await _userManager.GetRolesAsync(userEntity);

            return new AuthResponseDto
            {
                Email = userEntity.Email!,
                Role = roles.FirstOrDefault() ?? string.Empty,
                Token = token,
                ExpiresAt = expiresAt
            };
        }
    }
}