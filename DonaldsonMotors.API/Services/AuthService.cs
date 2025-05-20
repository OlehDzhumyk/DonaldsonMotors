// Services/AuthService.cs
using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Domain.Models;
using DonaldsonMotors.API.DTOs.Auth;
using DonaldsonMotors.API.Interfaces.Services;
using DonaldsonMotors.API.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DonaldsonMotors.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userMgr;
        private readonly RoleManager<ApplicationRole> _roleMgr;
        private readonly IConfiguration _config;

        public AuthService(
            UserManager<ApplicationUser> userMgr,
            RoleManager<ApplicationRole> roleMgr,
            IConfiguration config)
        {
            _userMgr = userMgr;
            _roleMgr = roleMgr;
            _config = config;
        }

        public async Task<RegisterResponseDto> RegisterCustomerAsync(RegisterRequestDto dto)
        {
            dto.Role = Roles.Customer;
            var token = await CreateUserAndTokenAsync(dto);
            return BuildRegisterResponse(token);
        }

        public async Task<RegisterResponseDto> RegisterStaffAsync(RegisterRequestDto dto)
        {
            if (dto.Role == Roles.Customer)
                throw new InvalidOperationException("Use RegisterCustomerAsync for Customers");

            var token = await CreateUserAndTokenAsync(dto);
            return BuildRegisterResponse(token);
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
        {
            var user = await _userMgr.FindByEmailAsync(dto.Email)
                       ?? throw new UnauthorizedAccessException("Invalid credentials");

            if (!await _userMgr.CheckPasswordAsync(user, dto.Password))
                throw new UnauthorizedAccessException("Invalid credentials");

            var jwt = await GenerateJwtTokenAsync(user);
            return BuildLoginResponse(jwt);
        }

        private async Task<string> CreateUserAndTokenAsync(RegisterRequestDto dto)
        {
            // Map incoming DTO to EF entity
            var userEntity = dto.ToEntity();

            var cr = await _userMgr.CreateAsync(userEntity, dto.Password);
            if (!cr.Succeeded)
                throw new InvalidOperationException(
                    string.Join(';', cr.Errors.Select(e => e.Description)));

            if (!await _roleMgr.RoleExistsAsync(dto.Role))
                await _roleMgr.CreateAsync(new ApplicationRole { Name = dto.Role });

            await _userMgr.AddToRoleAsync(userEntity, dto.Role);

            return await GenerateJwtTokenAsync(userEntity);
        }

        private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);
            var creds = new SigningCredentials(
                new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email!),
                new(ClaimTypes.Name, user.FullName!)
            };

            var roles = await _userMgr.GetRolesAsync(user);
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var jwt = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    int.Parse(_config["Jwt:ExpiryMinutes"]!)),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        private RegisterResponseDto BuildRegisterResponse(string token)
        {
            var expiresIn = int.Parse(_config["Jwt:ExpiryMinutes"]!);
            return new RegisterResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiresIn)
            };
        }

        private LoginResponseDto BuildLoginResponse(string token)
        {
            var expiresIn = int.Parse(_config["Jwt:ExpiryMinutes"]!);
            return new LoginResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiresIn)
            };
        }
    }
}