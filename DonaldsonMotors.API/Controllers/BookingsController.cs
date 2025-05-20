// Controllers/BookingsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DonaldsonMotors.API.Interfaces.Services;
using DonaldsonMotors.API.DTOs.Booking;
using DonaldsonMotors.API.Domain.Models;
using DonaldsonMotors.API.Mappers;

namespace DonaldsonMotors.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = Roles.Customer + "," + Roles.Manager + "," + Roles.Mechanic)]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingSvc;

        public BookingsController(IBookingService bookingSvc)
        {
            _bookingSvc = bookingSvc;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        [Authorize(Roles = Roles.Customer)]
        public async Task<IActionResult> Create([FromBody] CreateBookingRequestDto dto)
        {
            // Map up-front
            var model = dto.ToDomainModel();
            // assign the current customer
            model.Customer = new Customer { Id = GetUserId() };

            var created = await _bookingSvc.CreateAsync(model);
            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id, version = "1.0" },
                created.ToResponseDto()
            );
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            IEnumerable<Booking> bookings;

            if (User.IsInRole(Roles.Manager) || User.IsInRole(Roles.Mechanic))
                bookings = await _bookingSvc.ListAllAsync();
            else
                bookings = await _bookingSvc.ListByCustomerAsync(GetUserId());

            return Ok(bookings.Select(b => b.ToResponseDto()));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var booking = await _bookingSvc.GetByIdAsync(id);
            if (booking == null) return NotFound();

            var userId = GetUserId();
            if (!User.IsInRole(Roles.Manager) && booking.Customer.Id != userId)
                return Forbid();

            return Ok(booking.ToResponseDto());
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = Roles.Customer + "," + Roles.Manager)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBookingRequestDto dto)
        {
            var model = dto.ToDomainModel();
            // optionally preserve the customer/vehicle — only editable fields are overwritten by your mapper
            model.Customer = new Customer { Id = GetUserId() };

            // enforce row‑level security in controller:
            var existing = await _bookingSvc.GetByIdAsync(id);
            if (existing == null) return NotFound();
            if (!User.IsInRole(Roles.Manager) && existing.Customer.Id != GetUserId())
                return Forbid();

            var ok = await _bookingSvc.UpdateAsync(id, model);
            return ok ? NoContent() : NotFound();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = Roles.Customer + "," + Roles.Manager)]
        public async Task<IActionResult> Cancel(int id)
        {
            var existing = await _bookingSvc.GetByIdAsync(id);
            if (existing == null) return NotFound();
            if (!User.IsInRole(Roles.Manager) && existing.Customer.Id != GetUserId())
                return Forbid();

            var ok = await _bookingSvc.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }
}
