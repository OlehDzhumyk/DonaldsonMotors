// File: DTOs/Booking/BookingResponseDto.cs
namespace DonaldsonMotors.API.DTOs.Booking
{
    /// <summary>
    /// DTO for returning detailed booking information, suitable for dashboards and detailed views.
    /// </summary>
    public class BookingResponseDto
    {
        public int Id { get; set; }
        public DateTime SlotStart { get; set; }
        public string Status { get; set; } = string.Empty;

        // Service Details
        public int ServiceTypeId { get; set; } // Crucial for fetching available mechanics
        public string ServiceTypeName { get; set; } = string.Empty;
        public decimal ServiceTypePrice { get; set; }
        public double ServiceDurationHours { get; set; } // Added for completeness

        // Vehicle Details
        public string VehicleRegistrationNumber { get; set; } = string.Empty;
        public string VehicleMake { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public int VehicleYear { get; set; }

        // Customer Details
        public int CustomerId { get; set; }
        public string CustomerFullName { get; set; } = string.Empty;
        public string? CustomerPhoneNumber { get; set; }
        public string? CustomerEmail { get; set; }

        // Mechanic Details (useful for dashboard)
        public int? MechanicId { get; set; }
        public string? MechanicName { get; set; }
    }
}