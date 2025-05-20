using DonaldsonMotors.API.Domain.Models;
using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.DTOs.Vehicle;

namespace DonaldsonMotors.API.Mappers
{
    public static class VehicleMappers
    {
        public static Vehicle ToDomainModel(this VehicleEntity e)
            => new()
            {
                Registration = e.RegistrationNumber,
                Make = e.Make,
                Model = e.Model,
                Year = e.Year,
                Mileage = e.Mileage
            };
         
        public static VehicleEntity ToEntity(this Vehicle d, int ownerId)
            => new()
            {
                RegistrationNumber = d.Registration,
                Make = d.Make,
                Model = d.Model,
                Year = d.Year,
                Mileage = d.Mileage,
                CustomerId = ownerId
            };

        public static VehicleResponseDto ToResponseDto(this Vehicle d)
            => new()
            {
                RegistrationNumber = d.Registration,
                Make = d.Make,
                Model = d.Model,
                Year = d.Year,
                Mileage = d.Mileage
            };

        public static Vehicle ToDomainModel(this VehicleResponseDto dto)
            => new()
            {
                Registration = dto.RegistrationNumber,
                Make = dto.Make,
                Model = dto.Model,
                Year = dto.Year,
                Mileage = dto.Mileage
            };

        // New mappers for CreateVehicleRequestDto and UpdateVehicleRequestDto

        public static Vehicle ToDomainModel(this CreateVehicleRequestDto dto)
            => new()
            {
                Registration = dto.RegistrationNumber,
                Make = dto.Make,
                Model = dto.Model,
                Year = dto.Year,
                Mileage = dto.Mileage
            };

        public static Vehicle ToDomainModel(this UpdateVehicleRequestDto dto)
            => new()
            {
                Make = dto.Make ?? string.Empty,
                Model = dto.Model ?? string.Empty,
                Year = dto.Year ?? 0, // Provide a default value for nullable int
                Mileage = dto.Mileage ?? 0 // Provide a default value for nullable int
            };

        // Extension to map from Entity to Response DTO (if needed)
        public static VehicleResponseDto ToResponseDto(this VehicleEntity entity)
            => new()
            {
                RegistrationNumber = entity.RegistrationNumber,
                Make = entity.Make,
                Model = entity.Model,
                Year = entity.Year,
                Mileage = entity.Mileage
            };
    }
}