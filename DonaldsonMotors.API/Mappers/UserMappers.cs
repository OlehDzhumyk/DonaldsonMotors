using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.DTOs.User;
using DonaldsonMotors.API.DTOs.Vehicle; // For VehicleResponseDto

namespace DonaldsonMotors.API.Mappers
{
    public static class UserMapper
    {
        /// <summary>
        /// Maps a Customer entity to a UserProfileResponseDto.
        /// Assumes that the Customer's Vehicles collection has been loaded if vehicles are needed in the DTO.
        /// </summary>
        public static UserProfileResponseDto ToProfileResponseDto(this Customer customer)
        {
            if (customer == null) return null!; // Or throw ArgumentNullException

            return new UserProfileResponseDto
            {
                Id = customer.Id,
                FullName = customer.FullName,
                Email = customer.Email ?? string.Empty,
                Address = customer.Address,
                PhoneNumber = customer.PhoneNumber,
                // The mapper now directly uses the Vehicles property from the customer entity.
                // Ensure Vehicles are included when fetching the customer in your service if needed.
                Vehicles = customer.Vehicles?.Select(v => v.ToVehicleResponseDto()).ToList() ?? new List<VehicleResponseDto>()
            };
        }
    }
}