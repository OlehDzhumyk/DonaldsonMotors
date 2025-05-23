using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.DTOs.Vehicle;

namespace DonaldsonMotors.API.Mappers
{
    public static class VehicleMapper
    {
        public static Vehicle ToVehicleEntity(this VehicleRequestDto dto, int ownerId) => new()
        {
            RegistrationNumber = dto.RegistrationNumber,
            Make = dto.Make,
            Model = dto.Model,
            Year = dto.Year,
            Mileage = dto.Mileage,
            OwnerId = ownerId
        };

        public static void UpdateVehicleEntity(this Vehicle entity, VehicleRequestDto dto)
        {
            entity.Make = dto.Make;
            entity.Model = dto.Model;
            entity.Year = dto.Year;
            entity.Mileage = dto.Mileage;
        }

        public static VehicleResponseDto ToVehicleResponseDto(this Vehicle entity) => new()
        {
            RegistrationNumber = entity.RegistrationNumber,
            Make = entity.Make,
            Model = entity.Model,
            Year = entity.Year,
            Mileage = entity.Mileage
        };
    }
}