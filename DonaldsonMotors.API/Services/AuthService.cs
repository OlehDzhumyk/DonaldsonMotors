using DonaldsonMotors.API.Interfaces.Services;
using DonaldsonMotors.API.Models;
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

        public async Task<AuthResult> RegisterAsync(RegisterModel model)
        {
            // 1) Create correct subtype
            ApplicationUser user = model.Role == Roles.Customer
                ? new Customer()
                : (ApplicationUser)new Employee();

            user.UserName = model.Email;
            user.Email = model.Email;
            user.FullName = model.FullName;

            var createResult = await _userMgr.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = createResult.Errors.Select(e => e.Description)
                };

            // 2) Ensure the role exists
            if (!await _roleMgr.RoleExistsAsync(model.Role))
            {
                var role = new ApplicationRole
                {
                    Name = model.Role,
                    NormalizedName = model.Role.ToUpperInvariant()
                };
                await _roleMgr.CreateAsync(role);
            }

            // 3) Assign the user to that role
            await _userMgr.AddToRoleAsync(user, model.Role);

            // 4) Issue JWT
            var token = await GenerateJwtTokenAsync(user);
            return new AuthResult { Succeeded = true, Token = token };
        }

        public async Task<AuthResult> LoginAsync(LoginModel model)
        {
            var user = await _userMgr.FindByEmailAsync(model.Email);
            if (user == null || !await _userMgr.CheckPasswordAsync(user, model.Password))
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = new[] { "Invalid credentials." }
                };

            var token = await GenerateJwtTokenAsync(user);
            return new AuthResult { Succeeded = true, Token = token };
        }

        private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
            );
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email!),
                new(ClaimTypes.Name, user.FullName!)
            };

            var roles = await _userMgr.GetRolesAsync(user);
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    int.Parse(_config["Jwt:ExpiryMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
