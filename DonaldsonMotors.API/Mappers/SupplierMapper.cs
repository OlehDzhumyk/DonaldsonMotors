using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.DTOs.Supplier;

namespace DonaldsonMotors.API.Mappers
{
    public static class SupplierMapper
    {
        public static SupplierResponseDto ToSupplierResponseDto(this Supplier entity)
        {
            if (entity == null) return null!;
            return new SupplierResponseDto
            {
                Id = entity.Id,
                Name = entity.Name,
                AddressLine1 = entity.AddressLine1,
                AddressLine2 = entity.AddressLine2,
                Postcode = entity.Postcode,
                Telephone = entity.Telephone,
                Email = entity.Email
            };
        }

        public static Supplier ToSupplierEntity(this CreateSupplierRequestDto dto)
        {
            if (dto == null) return null!;
            return new Supplier
            {
                Name = dto.Name,
                AddressLine1 = dto.AddressLine1,
                AddressLine2 = dto.AddressLine2,
                Postcode = dto.Postcode,
                Telephone = dto.Telephone,
                Email = dto.Email
            };
        }

        public static void UpdateEntity(this Supplier entity, UpdateSupplierRequestDto dto)
        {
            if (dto == null || entity == null) return;

            entity.Name = dto.Name ?? entity.Name;
            entity.AddressLine1 = dto.AddressLine1 ?? entity.AddressLine1;
            entity.AddressLine2 = dto.AddressLine2 ?? entity.AddressLine2;
            entity.Postcode = dto.Postcode ?? entity.Postcode;
            entity.Telephone = dto.Telephone ?? entity.Telephone;
            entity.Email = dto.Email ?? entity.Email;
        }
    }
}