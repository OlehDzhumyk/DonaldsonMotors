using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.DTOs.ServiceType;

namespace DonaldsonMotors.API.Mappers
{
    public static class ServiceTypeMapper
    {
        public static ServiceTypeResponseDto ToServiceTypeResponseDto(this ServiceType entity)
        {
            if (entity == null) return null!;
            return new ServiceTypeResponseDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Price = entity.Price,
                DurationHours = entity.DurationHours
            };
        }

        public static ServiceType ToServiceTypeEntity(this CreateServiceTypeRequestDto dto)
        {
            if (dto == null) return null!;
            return new ServiceType
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                DurationHours = dto.DurationHours
            };
        }

        public static void UpdateEntity(this ServiceType entity, UpdateServiceTypeRequestDto dto)
        {
            if (dto == null || entity == null) return;

            entity.Name = dto.Name ?? entity.Name;
            entity.Description = dto.Description ?? entity.Description;
            entity.Price = dto.Price ?? entity.Price;
            entity.DurationHours = dto.DurationHours ?? entity.DurationHours;
        }
    }
}