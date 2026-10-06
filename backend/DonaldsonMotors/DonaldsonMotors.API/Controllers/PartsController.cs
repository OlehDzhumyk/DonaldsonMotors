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
    [Authorize(Roles = $"{Roles.StockController},{Roles.Manager},{Roles.Mechanic}")]
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
        // Override class-level authorization to include Mechanics for this GET endpoint
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
        // Override class-level authorization to include Mechanics for this GET endpoint
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
                return NotFound($"Part with ID {id} not found.");
            }
            return Ok(part);
        }

        // POST, PUT, DELETE, PATCH methods remain restricted to Manager and StockController 
        // by the class-level [Authorize] attribute. No changes needed for them.

        /// <summary>
        /// Creates a new part. (Manager/StockController Only)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(PartResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreatePart([FromBody] CreatePartRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var createdPart = await _partService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetPartById), new { id = createdPart.Id }, createdPart);
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (DuplicateResourceException ex) { return Conflict(ex.Message); } // Assuming you defined this
            catch (InvalidOperationException ex) { return Conflict(ex.Message); } // Or more general conflict
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating part.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Updates an existing part. (Manager/StockController Only)
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(PartResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdatePart(int id, [FromBody] UpdatePartRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var updatedPart = await _partService.UpdateAsync(id, dto);
                if (updatedPart == null)
                {
                    return NotFound($"Part with ID {id} not found.");
                }
                return Ok(updatedPart);
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (DuplicateResourceException ex) { return Conflict(ex.Message); } // Assuming you defined this
            catch (InvalidOperationException ex) { return Conflict(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating part ID {PartId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Updates the stock level of a specific part. (Manager/StockController Only)
        /// </summary>
        [HttpPatch("{id}/stock")]
        [ProducesResponseType(typeof(PartResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStockLevel(int id, [FromBody] UpdateStockLevelRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var updatedPart = await _partService.UpdateStockLevelAsync(id, dto);
                if (updatedPart == null)
                {
                    return NotFound($"Part with ID {id} not found for stock update.");
                }
                return Ok(updatedPart);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock for part ID {PartId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Deletes a part by its ID. (Manager/StockController Only)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePart(int id)
        {
            try
            {
                var success = await _partService.DeleteAsync(id);
                if (!success)
                {
                    return NotFound($"Part with ID {id} not found.");
                }
                return NoContent();
            }
            catch (ResourceInUseException ex) { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting part ID {PartId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }
    }
}