using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.DTOs.ServiceType;
using DonaldsonMotors.API.Interfaces;
using DonaldsonMotors.API.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DonaldsonMotors.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceTypesController : ControllerBase
    {
        private readonly IServiceTypeService _serviceTypeService;
        private readonly ILogger<ServiceTypesController> _logger;

        public ServiceTypesController(IServiceTypeService serviceTypeService, ILogger<ServiceTypesController> logger)
        {
            _serviceTypeService = serviceTypeService;
            _logger = logger;
        }

        /// <summary>
        /// Gets a list of all available service types. This endpoint is publicly accessible.
        /// </summary>
        /// <returns>A list of all service types.</returns>
        /// <response code="200">Returns the list of service types.</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<ServiceTypeResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllServiceTypes()
        {
            var serviceTypes = await _serviceTypeService.GetAllAsync();
            return Ok(serviceTypes);
        }

        /// <summary>
        /// Gets a specific service type by its ID. Accessible by all authenticated users.
        /// </summary>
        /// <param name="id">The ID of the service type to retrieve.</param>
        /// <returns>The requested service type.</returns>
        /// <response code="200">Returns the requested service type.</response>
        /// <response code="404">If a service type with the specified ID is not found.</response>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ServiceTypeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetServiceTypeById(int id)
        {
            var serviceType = await _serviceTypeService.GetByIdAsync(id);
            if (serviceType == null)
            {
                return Problem(detail: $"Service Type with ID {id} not found.", statusCode: StatusCodes.Status404NotFound);
            }
            return Ok(serviceType);
        }

        /// <summary>
        /// Creates a new service type. Accessible only by Managers.
        /// </summary>
        /// <param name="dto">The details for the new service type.</param>
        /// <returns>The newly created service type.</returns>
        /// <response code="201">Returns the newly created service type.</response>
        /// <response code="400">If the request payload is invalid.</response>
        /// <response code="409">If a service type with the same name already exists.</response>
        [HttpPost]
        [Authorize(Roles = Roles.Manager)]
        [ProducesResponseType(typeof(ServiceTypeResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateServiceType([FromBody] CreateServiceTypeRequestDto dto)
        {
            var createdServiceType = await _serviceTypeService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetServiceTypeById), new { id = createdServiceType.Id }, createdServiceType);
        }

        /// <summary>
        /// Updates an existing service type. Accessible only by Managers.
        /// </summary>
        /// <param name="id">The ID of the service type to update.</param>
        /// <param name="dto">The updated details for the service type.</param>
        /// <returns>The updated service type.</returns>
        /// <response code="200">Returns the updated service type.</response>
        /// <response code="400">If the request payload is invalid.</response>
        /// <response code="404">If a service type with the specified ID is not found.</response>
        /// <response code="409">If the updated name conflicts with another existing service type.</response>
        [HttpPut("{id}")]
        [Authorize(Roles = Roles.Manager)]
        [ProducesResponseType(typeof(ServiceTypeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateServiceType(int id, [FromBody] UpdateServiceTypeRequestDto dto)
        {
            var updatedServiceType = await _serviceTypeService.UpdateAsync(id, dto);
            if (updatedServiceType == null)
                return Problem(detail: $"Service Type with ID {id} not found.", statusCode: StatusCodes.Status404NotFound);

            return Ok(updatedServiceType);
        }

        /// <summary>
        /// Deletes a service type. Accessible only by Managers.
        /// </summary>
        /// <param name="id">The ID of the service type to delete.</param>
        /// <response code="204">If the service type was deleted successfully.</response>
        /// <response code="400">If the service type cannot be deleted because it is currently in use by bookings.</response>
        /// <response code="404">If a service type with the specified ID is not found.</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Manager)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteServiceType(int id)
        {
            var success = await _serviceTypeService.DeleteAsync(id);
            if (!success)
                return Problem(detail: $"Service Type with ID {id} not found.", statusCode: StatusCodes.Status404NotFound);

            return NoContent();
        }
    }
}