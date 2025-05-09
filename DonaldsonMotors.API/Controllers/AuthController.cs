using Microsoft.AspNetCore.Mvc;
using DonaldsonMotors.API.Interfaces.Services;
using DonaldsonMotors.API.Models;

namespace DonaldsonMotors.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth) => _auth = auth;

        // POST api/v1/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            var res = await _auth.RegisterAsync(model);
            if (!res.Succeeded) return BadRequest(res.Errors);
            return Ok(new { token = res.Token });
        }

        // POST api/v1/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var res = await _auth.LoginAsync(model);
            if (!res.Succeeded) return Unauthorized(res.Errors);
            return Ok(new { token = res.Token });
        }
    }
}
