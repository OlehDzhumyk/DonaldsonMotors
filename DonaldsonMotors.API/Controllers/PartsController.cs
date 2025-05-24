using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.DTOs.Part;
using DonaldsonMotors.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DonaldsonMotors.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Secure all endpoints, accessible by StockController or Manager
    [Authorize(Roles = $"{Roles.StockController},{Roles.Manager}")]
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
        /// Gets a list of all parts, with optional filtering by search term or supplier.
        /// </summary>
        /// <param name="searchTerm">A term to search for in part name or barcode.</param>
        /// <param name="supplierId">Optional ID of the supplier to filter parts by.</param>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PartResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllParts([FromQuery] string? searchTerm, [FromQuery] int? supplierId)
        {
            var parts = await _partService.GetAllAsync(searchTerm, supplierId);
            return Ok(parts);
        }

        /// <summary>
        /// Gets a specific part by its ID.
        /// </summary>
        /// <param name="id">The ID of the part.</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PartResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPartById(int id)
        {
            var part = await _partService.GetByIdAsync(id);
            if (part == null)
            {
                return NotFound($"Part with ID {id} not found.");
            }
            return Ok(part);
        }

        /// <summary>
        /// Creates a new part.
        /// </summary>
        /// <param name="dto">The part creation data.</param>
        [HttpPost]
        [ProducesResponseType(typeof(PartResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // For validation errors or invalid SupplierId
        [ProducesResponseType(StatusCodes.Status409Conflict)]  // For duplicate name/barcode
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
            catch (ArgumentException ex) // e.g., SupplierId not found
            {
                _logger.LogWarning("Failed to create part due to invalid argument: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex) // e.g., Duplicate part name/barcode
            {
                _logger.LogWarning("Failed to create part due to conflict: {ErrorMessage}", ex.Message);
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while creating a part.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Updates an existing part.
        /// </summary>
        /// <param name="id">The ID of the part to update.</param>
        /// <param name="dto">The part update data.</param>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(PartResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)] // For duplicate name/barcode during update
        public async Task<IActionResult> UpdatePart(int id, [FromBody] UpdatePartRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Attempting to update part with ID: {PartId}", id);
            try
            {
                var updatedPart = await _partService.UpdateAsync(id, dto);
                if (updatedPart == null)
                {
                    return NotFound($"Part with ID {id} not found.");
                }
                return Ok(updatedPart);
            }
            catch (ArgumentException ex) // e.g., New SupplierId not found
            {
                _logger.LogWarning("Failed to update part {PartId} due to invalid argument: {ErrorMessage}", id, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex) // e.g., Duplicate name/barcode
            {
                _logger.LogWarning("Failed to update part {PartId} due to conflict: {ErrorMessage}", id, ex.Message);
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating part {PartId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Updates the stock level of a specific part.
        /// </summary>
        /// <param name="id">The ID of the part to update stock for.</param>
        /// <param name="dto">The stock level update data.</param>
        [HttpPatch("{id}/stock")] // Using PATCH as it's a partial update of the part resource
        [ProducesResponseType(typeof(PartResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // For invalid operations like negative stock
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStockLevel(int id, [FromBody] UpdateStockLevelRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Attempting to update stock for part ID: {PartId}, Change: {Change}", id, dto.ChangeInQuantity);
            try
            {
                var updatedPart = await _partService.UpdateStockLevelAsync(id, dto);
                if (updatedPart == null)
                {
                    return NotFound($"Part with ID {id} not found for stock update.");
                }
                return Ok(updatedPart);
            }
            catch (InvalidOperationException ex) // E.g., stock would go negative
            {
                _logger.LogWarning("Failed to update stock for part {PartId}: {ErrorMessage}", id, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating stock for part {PartId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }


        /// <summary>
        /// Deletes a part by its ID.
        /// </summary>
        /// <param name="id">The ID of the part to delete.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // If part is used in jobs
        public async Task<IActionResult> DeletePart(int id)
        {
            _logger.LogInformation("Attempting to delete part with ID: {PartId}", id);
            try
            {
                var success = await _partService.DeleteAsync(id);
                if (!success)
                {
                    return NotFound($"Part with ID {id} not found.");
                }
                return NoContent();
            }
            catch (InvalidOperationException ex) // Catching specific exception (e.g., part is used)
            {
                _logger.LogWarning("Failed to delete part {PartId}: {ErrorMessage}", id, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while deleting part {PartId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }
    }
}