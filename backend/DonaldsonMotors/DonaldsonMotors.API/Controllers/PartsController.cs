using DonaldsonMotors.API.Data.Entities; // Required for Roles
using DonaldsonMotors.API.DTOs.Part;
using DonaldsonMotors.API.Interfaces;
using DonaldsonMotors.API.Exceptions; // For custom exceptions if PartService throws them
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DonaldsonMotors.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PartsController : ControllerBase
    {
        private readonly IPartService _partService;
        private readonly ILogger<PartsController> _logger;

        public PartsController(IPartService partService, ILogger<PartsController> logger)
        {
            _partService = partService;
            _logger = logger;
        }

        /// <summary>
        /// Gets a list of all parts, with optional filtering.
        /// Accessible by Managers, Stock Controllers, and Mechanics.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PartResponseDto>), StatusCodes.Status200OK)]
        [Authorize(Roles = $"{Roles.StockController},{Roles.Manager},{Roles.Mechanic}")]
        public async Task<IActionResult> GetAllParts([FromQuery] string? searchTerm, [FromQuery] int? supplierId)
        {
            _logger.LogInformation("Attempting to fetch all parts with searchTerm: '{SearchTerm}', supplierId: {SupplierId}", searchTerm, supplierId);
            var parts = await _partService.GetAllAsync(searchTerm, supplierId);
            return Ok(parts);
        }

        /// <summary>
        /// Gets a specific part by its ID.
        /// Accessible by Managers, Stock Controllers, and Mechanics.
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = $"{Roles.StockController},{Roles.Manager},{Roles.Mechanic}")]
        [ProducesResponseType(typeof(PartResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPartById(int id)
        {
            _logger.LogInformation("Attempting to fetch part with ID: {PartId}", id);
            var part = await _partService.GetByIdAsync(id);
            if (part == null)
            {
                _logger.LogWarning("Part with ID {PartId} not found.", id);
                return Problem(detail: $"Part with ID {id} not found.", statusCode: StatusCodes.Status404NotFound);
            }
            return Ok(part);
        }

        // Mechanics may only read parts (to add them to a job); changes are for Managers and Stock Controllers.
        // Roles are set per action because stacked [Authorize] attributes must all pass.

        /// <summary>
        /// Creates a new part. (Manager/StockController Only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = $"{Roles.StockController},{Roles.Manager}")]
        [ProducesResponseType(typeof(PartResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreatePart([FromBody] CreatePartRequestDto dto)
        {
            var createdPart = await _partService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetPartById), new { id = createdPart.Id }, createdPart);
        }

        /// <summary>
        /// Updates an existing part. (Manager/StockController Only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = $"{Roles.StockController},{Roles.Manager}")]
        [ProducesResponseType(typeof(PartResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdatePart(int id, [FromBody] UpdatePartRequestDto dto)
        {
            var updatedPart = await _partService.UpdateAsync(id, dto);
            if (updatedPart == null)
            {
                return Problem(detail: $"Part with ID {id} not found.", statusCode: StatusCodes.Status404NotFound);
            }
            return Ok(updatedPart);
        }

        /// <summary>
        /// Updates the stock level of a specific part. (Manager/StockController Only)
        /// </summary>
        [HttpPatch("{id}/stock")]
        [Authorize(Roles = $"{Roles.StockController},{Roles.Manager}")]
        [ProducesResponseType(typeof(PartResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStockLevel(int id, [FromBody] UpdateStockLevelRequestDto dto)
        {
            var updatedPart = await _partService.UpdateStockLevelAsync(id, dto);
            if (updatedPart == null)
            {
                return Problem(detail: $"Part with ID {id} not found for stock update.", statusCode: StatusCodes.Status404NotFound);
            }
            return Ok(updatedPart);
        }

        /// <summary>
        /// Deletes a part by its ID. (Manager/StockController Only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = $"{Roles.StockController},{Roles.Manager}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePart(int id)
        {
            var success = await _partService.DeleteAsync(id);
            if (!success)
            {
                return Problem(detail: $"Part with ID {id} not found.", statusCode: StatusCodes.Status404NotFound);
            }
            return NoContent();
        }
    }
}