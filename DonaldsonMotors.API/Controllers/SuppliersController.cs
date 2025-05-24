using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.DTOs.Supplier;
using DonaldsonMotors.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DonaldsonMotors.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Secure all endpoints in this controller, accessible by StockController or Manager
    [Authorize(Roles = $"{Roles.StockController},{Roles.Manager}")]
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierService _supplierService;
        private readonly ILogger<SuppliersController> _logger;

        public SuppliersController(ISupplierService supplierService, ILogger<SuppliersController> logger)
        {
            _supplierService = supplierService;
            _logger = logger;
        }

        /// <summary>
        /// Gets a list of all suppliers.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SupplierResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllSuppliers()
        {
            var suppliers = await _supplierService.GetAllAsync();
            return Ok(suppliers);
        }

        /// <summary>
        /// Gets a specific supplier by its ID.
        /// </summary>
        /// <param name="id">The ID of the supplier.</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SupplierResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSupplierById(int id)
        {
            var supplier = await _supplierService.GetByIdAsync(id);
            if (supplier == null)
            {
                return NotFound($"Supplier with ID {id} not found.");
            }
            return Ok(supplier);
        }

        /// <summary>
        /// Creates a new supplier.
        /// </summary>
        /// <param name="dto">The supplier creation data.</param>
        [HttpPost]
        [ProducesResponseType(typeof(SupplierResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)] // For duplicate names
        public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdSupplier = await _supplierService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetSupplierById), new { id = createdSupplier.Id }, createdSupplier);
            }
            catch (InvalidOperationException ex) // Catching specific exception for duplicate name
            {
                _logger.LogWarning("Failed to create supplier: {ErrorMessage}", ex.Message);
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while creating a supplier.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Updates an existing supplier.
        /// </summary>
        /// <param name="id">The ID of the supplier to update.</param>
        /// <param name="dto">The supplier update data.</param>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(SupplierResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)] // For duplicate names
        public async Task<IActionResult> UpdateSupplier(int id, [FromBody] UpdateSupplierRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Attempting to update supplier with ID: {SupplierId}", id);
            try
            {
                var updatedSupplier = await _supplierService.UpdateAsync(id, dto);
                if (updatedSupplier == null)
                {
                    return NotFound($"Supplier with ID {id} not found.");
                }
                return Ok(updatedSupplier);
            }
            catch (InvalidOperationException ex) // Catching specific exception for duplicate name
            {
                _logger.LogWarning("Failed to update supplier {SupplierId}: {ErrorMessage}", id, ex.Message);
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating supplier {SupplierId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Deletes a supplier by its ID.
        /// </summary>
        /// <param name="id">The ID of the supplier to delete.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // If supplier has associated parts
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            _logger.LogInformation("Attempting to delete supplier with ID: {SupplierId}", id);
            try
            {
                var success = await _supplierService.DeleteAsync(id);
                if (!success)
                {
                    return NotFound($"Supplier with ID {id} not found.");
                }
                return NoContent();
            }
            catch (InvalidOperationException ex) // Catching specific exception (e.g., supplier has parts)
            {
                _logger.LogWarning("Failed to delete supplier {SupplierId}: {ErrorMessage}", id, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while deleting supplier {SupplierId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }
    }
}