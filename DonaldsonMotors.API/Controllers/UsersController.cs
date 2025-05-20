using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DonaldsonMotors.API.Interfaces.Services;
using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Domain.Models;

namespace DonaldsonMotors.API.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = Roles.Customer + "," + Roles.Manager)]
    [Route("api/v1/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _svc;

        public UsersController(IUserService svc) => _svc = svc;

        // Helper to get current user's ID from JWT
        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>GET api/v1/users/me</summary>
        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            var id = GetUserId();
            var user = await _svc.GetProfileAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        /// <summary>PUT api/v1/users/me</summary>
        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile([FromBody] Customer update)
        {
            var id = GetUserId();
            var ok = await _svc.UpdateProfileAsync(id,
                update.FullName,
                update.Address,
                update.TelephoneNumber);
            return ok ? NoContent() : NotFound();
        }

        /// <summary>POST api/v1/users/me/cars</summary>
        [HttpPost("me/cars")]
        public async Task<IActionResult> AddVehicle([FromBody] Vehicle vehicle)
        {
            var id = GetUserId();
            var created = await _svc.AddVehicleAsync(id, vehicle);
            return CreatedAtAction(nameof(GetProfile), new { }, created);
        }

        /// <summary>PUT api/v1/users/me/cars/{registration}</summary>
        [HttpPut("me/cars/{registration}")]
        public async Task<IActionResult> UpdateVehicle(string registration, [FromBody] Vehicle vehicle)
        {
            var id = GetUserId();
            var ok = await _svc.UpdateVehicleAsync(id, registration, vehicle);
            return ok ? NoContent() : NotFound();
        }

        /// <summary>DELETE api/v1/users/me/cars/{registration}</summary>
        [HttpDelete("me/cars/{registration}")]
        public async Task<IActionResult> DeleteVehicle(string registration)
        {
            var id = GetUserId();
            var ok = await _svc.DeleteVehicleAsync(id, registration);
            return ok ? NoContent() : NotFound();
        }
    }
}
