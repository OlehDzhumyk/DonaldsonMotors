using DonaldsonMotors.API.DTOs.Part;
using DonaldsonMotors.API.Interfaces;
using DonaldsonMotors.API.Mappers; // Ensure your mappers are in this namespace

namespace DonaldsonMotors.API.Services
{
    public class PartService : IPartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PartService> _logger;

        public PartService(IUnitOfWork unitOfWork, ILogger<PartService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<PartResponseDto>> GetAllAsync(string? searchTerm, int? supplierId)
        {
            _logger.LogInformation("Fetching all parts. SearchTerm: {SearchTerm}, SupplierId: {SupplierId}", searchTerm, supplierId);
            var parts = await _unitOfWork.Parts.GetAllWithSupplierAsync();

            if (supplierId.HasValue)
            {
                parts = parts.Where(p => p.SupplierId == supplierId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLowerInvariant();
                parts = parts.Where(p =>
                    p.Name.ToLowerInvariant().Contains(term) ||
                    (p.Barcode != null && p.Barcode.ToLowerInvariant().Contains(term)));
            }

            return parts.Select(p => p.ToPartResponseDto());
        }

        public async Task<PartResponseDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching part with ID {PartId}.", id);
            var part = await _unitOfWork.Parts.GetByIdWithSupplierAsync(id);
            return part?.ToPartResponseDto();
        }

        public async Task<PartResponseDto> CreateAsync(CreatePartRequestDto dto)
        {
            _logger.LogInformation("Attempting to create new part: {PartName}", dto.Name);

            // Validate if supplier exists
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(dto.SupplierId)
                ?? throw new ArgumentException($"Supplier with ID {dto.SupplierId} not found.");

            // Optional: Check for existing part with the same name or barcode
            var existingPart = (await _unitOfWork.Parts.FindAsync(p =>
                p.Name.ToLower() == dto.Name.ToLower() ||
                (!string.IsNullOrEmpty(dto.Barcode) && p.Barcode == dto.Barcode)
            )).FirstOrDefault();

            if (existingPart != null)
            {
                var message = $"A part with the same name or barcode already exists (ID: {existingPart.Id}).";
                _logger.LogWarning(message);
                throw new InvalidOperationException(message);
            }

            var partEntity = dto.ToPartEntity(); // Using mapper

            await _unitOfWork.Parts.AddAsync(partEntity);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Successfully created part {PartName} with ID {PartId}.", partEntity.Name, partEntity.Id);
            // To include SupplierName in response, we need to fetch it or assign it
            partEntity.Supplier = supplier; // Assign the fetched supplier
            return partEntity.ToPartResponseDto();
        }

        public async Task<PartResponseDto?> UpdateAsync(int id, UpdatePartRequestDto dto)
        {
            _logger.LogInformation("Attempting to update part with ID {PartId}.", id);
            var existingPart = await _unitOfWork.Parts.GetByIdWithSupplierAsync(id);

            if (existingPart == null)
            {
                _logger.LogWarning("Part with ID {PartId} not found for update.", id);
                return null;
            }

            // If SupplierId is being changed, validate the new supplier
            if (dto.SupplierId.HasValue && dto.SupplierId.Value != existingPart.SupplierId)
            {
                var newSupplier = await _unitOfWork.Suppliers.GetByIdAsync(dto.SupplierId.Value)
                    ?? throw new ArgumentException($"New Supplier with ID {dto.SupplierId.Value} not found.");
                existingPart.Supplier = newSupplier; // Update the navigation property if needed for mapper
            }

            // Optional: Check for name/barcode conflict if they are being changed
            if (!string.IsNullOrEmpty(dto.Name) && !existingPart.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase))
            {
                var conflicting = (await _unitOfWork.Parts.FindAsync(p => p.Name.ToLower() == dto.Name.ToLower() && p.Id != id)).FirstOrDefault();
                if (conflicting != null) throw new InvalidOperationException($"Another part with name '{dto.Name}' already exists.");
            }
            if (!string.IsNullOrEmpty(dto.Barcode) && !string.Equals(existingPart.Barcode, dto.Barcode, StringComparison.OrdinalIgnoreCase))
            {
                var conflicting = (await _unitOfWork.Parts.FindAsync(p => p.Barcode == dto.Barcode && p.Id != id)).FirstOrDefault();
                if (conflicting != null) throw new InvalidOperationException($"Another part with barcode '{dto.Barcode}' already exists.");
            }


            existingPart.UpdateEntity(dto); // Using mapper to update fields
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Successfully updated part with ID {PartId}.", existingPart.Id);
            return existingPart.ToPartResponseDto();
        }

        public async Task<PartResponseDto?> UpdateStockLevelAsync(int id, UpdateStockLevelRequestDto dto)
        {
            _logger.LogInformation("Attempting to update stock level for part ID {PartId}. Change: {Change}", id, dto.ChangeInQuantity);
            var part = await _unitOfWork.Parts.GetByIdWithSupplierAsync(id); // Fetch with supplier for response DTO

            if (part == null)
            {
                _logger.LogWarning("Part with ID {PartId} not found for stock update.", id);
                return null;
            }

            var newStockLevel = part.CurrentStockLevel + dto.ChangeInQuantity;
            if (newStockLevel < 0)
            {
                _logger.LogWarning("Stock update for Part ID {PartId} would result in negative stock ({NewStockLevel}).", id, newStockLevel);
                throw new InvalidOperationException("Stock level cannot be negative.");
            }

            part.CurrentStockLevel = newStockLevel;
            // Here you could add a record to a StockMovementLog if you had one:
            // await _unitOfWork.StockMovementLogs.AddAsync(new StockMovementLog { PartId = id, QuantityChanged = dto.ChangeInQuantity, Reason = dto.Reason, ChangeDate = DateTime.UtcNow });

            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Stock level updated for part ID {PartId}. New Level: {NewStockLevel}", id, part.CurrentStockLevel);

            return part.ToPartResponseDto();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Attempting to delete part with ID {PartId}.", id);
            var part = await _unitOfWork.Parts.GetByIdAsync(id);

            if (part == null)
            {
                _logger.LogWarning("Part with ID {PartId} not found for deletion.", id);
                return false;
            }

            // Check if part is used in any JobParts
            var isUsed = (await _unitOfWork.JobParts.FindAsync(jp => jp.PartId == id)).Any();
            if (isUsed)
            {
                _logger.LogWarning("Cannot delete part {PartId} as it is used in existing jobs.", id);
                throw new InvalidOperationException("Cannot delete part: it has been used in jobs. Consider marking it as inactive instead.");
            }

            _unitOfWork.Parts.Delete(part);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Successfully deleted part with ID {PartId}.", id);
            return true;
        }
    }
}