using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DonaldsonMotors.API.Interfaces;
using DonaldsonMotors.API.Models;

namespace DonaldsonMotors.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _svc;

        public BookingsController(IBookingService svc) => _svc = svc;

        // GET api/v1/bookings
        [HttpGet, Authorize]
        public async Task<IActionResult> GetAll()
            => Ok(await _svc.GetAllAsync());

        // GET api/v1/bookings/{id}
        [HttpGet("{id:int}"), Authorize]
        public async Task<IActionResult> Get(int id)
        {
            var b = await _svc.GetByIdAsync(id);
            return b == null ? NotFound() : Ok(b);
        }

        // POST api/v1/bookings
        [HttpPost, Authorize(Roles = $"{Roles.Customer},{Roles.Manager}")]
        public async Task<IActionResult> Create([FromBody] Booking model)
        {
            var created = await _svc.CreateAsync(model);
            return CreatedAtAction(nameof(Get), new { id = created.Id, version = "1.0" }, created);
        }

        // PUT api/v1/bookings/{id}
        [HttpPut("{id:int}"), Authorize(Roles = $"{Roles.Customer},{Roles.Manager}")]
        public async Task<IActionResult> Update(int id, [FromBody] Booking model)
            => await _svc.UpdateAsync(id, model)
               ? NoContent()
               : NotFound();

        // DELETE api/v1/bookings/{id}
        [HttpDelete("{id:int}"), Authorize(Roles = $"{Roles.Customer},{Roles.Manager}")]
        public async Task<IActionResult> Delete(int id)
            => await _svc.DeleteAsync(id)
               ? NoContent()
               : NotFound();
    }
}
