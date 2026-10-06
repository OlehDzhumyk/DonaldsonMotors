using DonaldsonMotors.API.DTOs.ServiceType;
using DonaldsonMotors.API.Interfaces;
using DonaldsonMotors.API.Mappers;
using DonaldsonMotors.API.Exceptions; // For potential custom exceptions

namespace DonaldsonMotors.API.Services
{
    public class ServiceTypeService : IServiceTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ServiceTypeService> _logger;

        public ServiceTypeService(IUnitOfWork unitOfWork, ILogger<ServiceTypeService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<ServiceTypeResponseDto>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all service types.");
            var serviceTypes = await _unitOfWork.ServiceTypes.GetAllAsync();
            return serviceTypes.Select(st => st.ToServiceTypeResponseDto());
        }

        public async Task<ServiceTypeResponseDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching service type with ID {Id}", id);
            var serviceType = await _unitOfWork.ServiceTypes.GetByIdAsync(id);
            return serviceType?.ToServiceTypeResponseDto();
        }

        public async Task<ServiceTypeResponseDto> CreateAsync(CreateServiceTypeRequestDto dto)
        {
            _logger.LogInformation("Creating new service type with name: {Name}", dto.Name);

            // Optional: Check for duplicate name
            var existing = (await _unitOfWork.ServiceTypes.FindAsync(st => st.Name.ToLower() == dto.Name.ToLower())).FirstOrDefault();
            if (existing != null)
            {
                throw new DuplicateResourceException($"A service type with the name '{dto.Name}' already exists.");
            }

            var serviceTypeEntity = dto.ToServiceTypeEntity();
            await _unitOfWork.ServiceTypes.AddAsync(serviceTypeEntity);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Service type created successfully with ID {Id}", serviceTypeEntity.Id);
            return serviceTypeEntity.ToServiceTypeResponseDto();
        }

        public async Task<ServiceTypeResponseDto?> UpdateAsync(int id, UpdateServiceTypeRequestDto dto)
        {
            _logger.LogInformation("Updating service type with ID {Id}", id);
            var serviceTypeEntity = await _unitOfWork.ServiceTypes.GetByIdAsync(id);
            if (serviceTypeEntity == null)
            {
                _logger.LogWarning("Service type with ID {Id} not found for update.", id);
                return null; // Controller will handle this as NotFound
            }

            // Optional: Check for duplicate name if name is being changed
            if (!string.IsNullOrWhiteSpace(dto.Name) && !serviceTypeEntity.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase))
            {
                var existing = (await _unitOfWork.ServiceTypes.FindAsync(st => st.Name.ToLower() == dto.Name.ToLower() && st.Id != id)).FirstOrDefault();
                if (existing != null)
                {
                    throw new DuplicateResourceException($"Another service type with the name '{dto.Name}' already exists.");
                }
            }

            serviceTypeEntity.UpdateEntity(dto); // Apply updates from DTO
            // _unitOfWork.ServiceTypes.Update(serviceTypeEntity); // EF Core usually tracks changes
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Service type with ID {Id} updated successfully.", id);
            return serviceTypeEntity.ToServiceTypeResponseDto();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Attempting to delete service type with ID {Id}", id);
            var serviceTypeEntity = await _unitOfWork.ServiceTypes.GetByIdAsync(id);
            if (serviceTypeEntity == null)
            {
                _logger.LogWarning("Service type with ID {Id} not found for deletion.", id);
                return false; // Controller will handle as NotFound
            }

            // CRUCIAL CHECK: Ensure this service type is not used in any bookings
            var bookingsUsingServiceType = await _unitOfWork.Bookings.FindAsync(b => b.ServiceTypeId == id);
            if (bookingsUsingServiceType.Any())
            {
                _logger.LogWarning("Cannot delete service type ID {Id} as it is currently used in {Count} bookings.", id, bookingsUsingServiceType.Count());
                throw new ResourceInUseException($"Cannot delete service type '{serviceTypeEntity.Name}' (ID: {id}) as it is currently associated with existing bookings. Please update or remove these bookings first.");
            }

            _unitOfWork.ServiceTypes.Delete(serviceTypeEntity);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Service type with ID {Id} deleted successfully.", id);
            return true;
        }
    }
}