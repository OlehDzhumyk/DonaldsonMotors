using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Domain.Models;
using DonaldsonMotors.API.DTOs.Auth;
using DonaldsonMotors.API.Interfaces.Services;
using DonaldsonMotors.API.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace DonaldsonMotors.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userMgr;
        private readonly RoleManager<ApplicationRole> _roleMgr;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userMgr,
            RoleManager<ApplicationRole> roleMgr,
            IJwtService jwtService,
            ILogger<AuthService> logger)
        {
            _userMgr = userMgr;
            _roleMgr = roleMgr;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<RegisterResponseDto> RegisterCustomerAsync(RegisterRequestDto dto)
        {
            _logger.LogDebug("📥 RegisterCustomerAsync for {Email}", dto.Email);
            dto.Role = Roles.Customer;
            return await CreateUserAndReturnToken(dto);
        }

        public async Task<RegisterResponseDto> RegisterStaffAsync(RegisterRequestDto dto)
        {
            _logger.LogDebug("📥 RegisterStaffAsync for {Email} with Role {Role}", dto.Email, dto.Role);

            if (dto.Role == Roles.Customer)
                throw new InvalidOperationException("Use RegisterCustomerAsync for Customers");

            return await CreateUserAndReturnToken(dto);
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
        {
            _logger.LogDebug("📥 LoginAsync for {Email}", dto.Email);

            var userEntity = await _userMgr.FindByEmailAsync(dto.Email)
                       ?? throw new UnauthorizedAccessException("Invalid credentials");

            if (!await _userMgr.CheckPasswordAsync(userEntity, dto.Password))
                throw new UnauthorizedAccessException("Invalid credentials");

            _logger.LogInformation("🔐 Password matched for {Email}", dto.Email);

            var (token, expiresAt) = await _jwtService.GenerateTokenAsync(userEntity);


            return new LoginResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt
            };
        }

        private async Task<RegisterResponseDto> CreateUserAndReturnToken(RegisterRequestDto dto)
        {
            var userEntity = dto.ToEntity();
            var cr = await _userMgr.CreateAsync(userEntity, dto.Password);

            if (!cr.Succeeded)
            {
                var errors = string.Join("; ", cr.Errors.Select(e => e.Description));
                _logger.LogWarning("⚠️ User creation failed: {Email}, Errors: {Errors}", dto.Email, errors);
                throw new InvalidOperationException(errors);
            }

            if (!await _roleMgr.RoleExistsAsync(dto.Role))
            {
                await _roleMgr.CreateAsync(new ApplicationRole { Name = dto.Role });
                _logger.LogInformation("🛠️ Created new role: {Role}", dto.Role);
            }

            await _userMgr.AddToRoleAsync(userEntity, dto.Role);
            _logger.LogInformation("🧾 Assigned role {Role} to user {Email}", dto.Role, dto.Email);

            var (token, expiresAt) = await _jwtService.GenerateTokenAsync(userEntity);

            return new RegisterResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt
            };
        }
    }
}
