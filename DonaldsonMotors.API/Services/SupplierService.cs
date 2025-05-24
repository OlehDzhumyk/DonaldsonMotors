using DonaldsonMotors.API.DTOs.Supplier;
using DonaldsonMotors.API.Interfaces;
using DonaldsonMotors.API.Mappers; // Ensure your mappers are in this namespace

namespace DonaldsonMotors.API.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SupplierService> _logger;

        public SupplierService(IUnitOfWork unitOfWork, ILogger<SupplierService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<SupplierResponseDto>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all suppliers.");
            var suppliers = await _unitOfWork.Suppliers.GetAllAsync();
            return suppliers.Select(s => s.ToSupplierResponseDto());
        }

        public async Task<SupplierResponseDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching supplier with ID {SupplierId}.", id);
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
            return supplier?.ToSupplierResponseDto();
        }

        public async Task<SupplierResponseDto> CreateAsync(CreateSupplierRequestDto dto)
        {
            _logger.LogInformation("Attempting to create new supplier with name {SupplierName}.", dto.Name);

            // Optional: Check for existing supplier with the same name to prevent duplicates
            var existingSupplier = (await _unitOfWork.Suppliers.FindAsync(s => s.Name.ToLower() == dto.Name.ToLower())).FirstOrDefault();
            if (existingSupplier != null)
            {
                _logger.LogWarning("Supplier with name {SupplierName} already exists.", dto.Name);
                throw new InvalidOperationException($"A supplier with the name '{dto.Name}' already exists.");
            }

            var supplierEntity = dto.ToSupplierEntity(); // Using mapper

            await _unitOfWork.Suppliers.AddAsync(supplierEntity);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Successfully created supplier with ID {SupplierId}.", supplierEntity.Id);
            return supplierEntity.ToSupplierResponseDto();
        }

        public async Task<SupplierResponseDto?> UpdateAsync(int id, UpdateSupplierRequestDto dto)
        {
            _logger.LogInformation("Attempting to update supplier with ID {SupplierId}.", id);
            var existingSupplier = await _unitOfWork.Suppliers.GetByIdAsync(id);

            if (existingSupplier == null)
            {
                _logger.LogWarning("Supplier with ID {SupplierId} not found for update.", id);
                return null; // Or throw KeyNotFoundException
            }

            // Optional: Check if new name conflicts with another existing supplier
            if (!string.IsNullOrEmpty(dto.Name) && !existingSupplier.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase))
            {
                var conflictingSupplier = (await _unitOfWork.Suppliers.FindAsync(s => s.Name.ToLower() == dto.Name.ToLower() && s.Id != id)).FirstOrDefault();
                if (conflictingSupplier != null)
                {
                    _logger.LogWarning("Update failed: another supplier with name {SupplierName} already exists.", dto.Name);
                    throw new InvalidOperationException($"Another supplier with the name '{dto.Name}' already exists.");
                }
            }

            existingSupplier.UpdateEntity(dto); // Using mapper to update fields

            //_unitOfWork.Suppliers.Update(existingSupplier); // EF Core tracks changes, explicit Update might not be needed if entity is tracked.
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Successfully updated supplier with ID {SupplierId}.", existingSupplier.Id);
            return existingSupplier.ToSupplierResponseDto();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Attempting to delete supplier with ID {SupplierId}.", id);
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);

            if (supplier == null)
            {
                _logger.LogWarning("Supplier with ID {SupplierId} not found for deletion.", id);
                return false; // Or throw KeyNotFoundException
            }

            // Optional: Check if supplier has associated parts before deleting
            var hasParts = await _unitOfWork.Parts.FindAsync(p => p.SupplierId == id);
            if (hasParts.Any())
            {
                _logger.LogWarning("Cannot delete supplier {SupplierId} as it has associated parts.", id);
                throw new InvalidOperationException("Cannot delete supplier: it has associated parts. Please reassign or delete parts first.");
            }

            _unitOfWork.Suppliers.Delete(supplier);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Successfully deleted supplier with ID {SupplierId}.", id);
            return true;
        }
    }
}