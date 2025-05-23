using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.DTOs.Booking;

namespace DonaldsonMotors.API.Mappers
{
    public static class BookingMapper
    {
        /// <summary>
        /// Maps a Booking entity to a BookingResponseDto.
        /// Handles cases where navigation properties like ServiceType might not be loaded.
        /// </summary>
        public static BookingResponseDto ToResponseDto(this Booking booking)
        {
            return new BookingResponseDto
            {
                Id = booking.Id,
                SlotStart = booking.SlotStart,
                Status = booking.Status.ToString(),
                VehicleRegistrationNumber = booking.VehicleRegistrationNumber,
                // Safely access related data
                ServiceTypeName = booking.ServiceType?.Name ?? "N/A",
                ServiceTypePrice = booking.ServiceType?.Price ?? 0m
            };
        }
    }
}