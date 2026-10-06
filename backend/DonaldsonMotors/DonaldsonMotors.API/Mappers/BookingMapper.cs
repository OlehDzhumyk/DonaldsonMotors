// File: Mappers/BookingMapper.cs
using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.DTOs.Booking;

namespace DonaldsonMotors.API.Mappers
{
    public static class BookingMapper
    {
        /// <summary>
        /// Maps a Booking entity to a detailed BookingResponseDto.
        /// </summary>
        public static BookingResponseDto ToResponseDto(this Booking booking)
        {
            if (booking == null) return null!;


            return new BookingResponseDto
            {
                Id = booking.Id,
                SlotStart = booking.SlotStart,
                Status = booking.Status.ToString(),

                ServiceTypeId = booking.ServiceTypeId,
                ServiceTypeName = booking.ServiceType?.Name ?? "N/A",
                ServiceTypePrice = booking.ServiceType?.Price ?? 0m,
                ServiceDurationHours = booking.ServiceType?.DurationHours ?? 0,

                VehicleRegistrationNumber = booking.VehicleRegistrationNumber,
                VehicleMake = booking.Vehicle?.Make ?? "N/A",
                VehicleModel = booking.Vehicle?.Model ?? "N/A",
                VehicleYear = booking.Vehicle?.Year ?? 0,

                CustomerId = booking.CustomerId,
                CustomerFullName = booking.Customer?.FullName ?? "N/A",
                CustomerPhoneNumber = booking.Customer?.PhoneNumber,
                CustomerEmail = booking.Customer?.Email,

                MechanicId = booking.MechanicId,
                MechanicName = null 
            };
        }
    }
}