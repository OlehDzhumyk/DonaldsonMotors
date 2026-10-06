using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.DTOs.Part;

namespace DonaldsonMotors.API.Mappers
{
    public static class PartMapper
    {
        public static PartResponseDto ToPartResponseDto(this Part entity)
        {
            if (entity == null) return null!;
            return new PartResponseDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Price = entity.Price,
                CostPrice = entity.CostPrice,
                CurrentStockLevel = entity.CurrentStockLevel,
                Barcode = entity.Barcode,
                SupplierId = entity.SupplierId,
                SupplierName = entity.Supplier?.Name ?? "N/A" // Safely access supplier name
            };
        }

        public static Part ToPartEntity(this CreatePartRequestDto dto)
        {
            if (dto == null) return null!;
            return new Part
            {
                Name = dto.Name,
                Price = dto.Price,
                CostPrice = dto.CostPrice,
                CurrentStockLevel = dto.InitialStockLevel,
                Barcode = dto.Barcode,
                SupplierId = dto.SupplierId
            };
        }

        public static void UpdateEntity(this Part entity, UpdatePartRequestDto dto)
        {
            if (dto == null || entity == null) return;

            entity.Name = dto.Name ?? entity.Name;
            entity.Price = dto.Price ?? entity.Price;
            entity.CostPrice = dto.CostPrice ?? entity.CostPrice;
            entity.Barcode = dto.Barcode ?? entity.Barcode;
            entity.SupplierId = dto.SupplierId ?? entity.SupplierId;
        }
    }
}