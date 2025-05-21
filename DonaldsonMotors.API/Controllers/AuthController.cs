using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DonaldsonMotors.API.DTOs.Auth;
using DonaldsonMotors.API.Interfaces.Services;
using DonaldsonMotors.API.Domain.Models;

namespace DonaldsonMotors.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService auth, ILogger<AuthController> logger)
        {
            _auth = auth;
            _logger = logger;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<RegisterResponseDto>> RegisterCustomer([FromBody] RegisterRequestDto dto)
        {
            _logger.LogInformation("🔵 Registering new customer: {Email}", dto.Email);

            try
            {
                var resp = await _auth.RegisterCustomerAsync(dto);
                _logger.LogInformation("✅ Customer registered: {Email}", dto.Email);
                return Ok(resp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to register customer: {Email}", dto.Email);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("register/staff")]
        [Authorize(Roles = Roles.Manager)]
        public async Task<ActionResult<RegisterResponseDto>> RegisterStaff([FromBody] RegisterRequestDto dto)
        {
            _logger.LogInformation("🔵 Registering new staff: {Email}, Role: {Role}", dto.Email, dto.Role);

            try
            {
                var resp = await _auth.RegisterStaffAsync(dto);
                _logger.LogInformation("✅ Staff registered: {Email}, Role: {Role}", dto.Email, dto.Role);
                return Ok(resp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to register staff: {Email}", dto.Email);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto dto)
        {
            _logger.LogInformation("🔵 Login attempt for: {Email}", dto.Email);

            try
            {
                var resp = await _auth.LoginAsync(dto);
                _logger.LogInformation("✅ Login success: {Email}", dto.Email);
                return Ok(resp);
            }
            catch (UnauthorizedAccessException uex)
            {
                _logger.LogWarning("⚠️ Invalid login attempt: {Email}", dto.Email);
                return Unauthorized(uex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Unexpected error during login: {Email}", dto.Email);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}
