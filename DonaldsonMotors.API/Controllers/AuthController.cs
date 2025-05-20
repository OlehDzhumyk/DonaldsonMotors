// Controllers/AuthController.cs
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

        public AuthController(IAuthService auth) => _auth = auth;

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<RegisterResponseDto>> RegisterCustomer(
            [FromBody] RegisterRequestDto dto)
        {
            var resp = await _auth.RegisterCustomerAsync(dto);
            return Ok(resp);
        }

        [HttpPost("register/staff")]
        [Authorize(Roles = Roles.Manager)]
        public async Task<ActionResult<RegisterResponseDto>> RegisterStaff(
            [FromBody] RegisterRequestDto dto)
        {
            var resp = await _auth.RegisterStaffAsync(dto);
            return Ok(resp);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> Login(
            [FromBody] LoginRequestDto dto)
        {
            var resp = await _auth.LoginAsync(dto);
            return Ok(resp);
        }
    }
}
