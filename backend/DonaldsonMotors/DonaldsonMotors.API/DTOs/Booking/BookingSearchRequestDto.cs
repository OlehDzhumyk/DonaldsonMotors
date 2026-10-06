using System; // Required for DateTime

namespace DonaldsonMotors.API.DTOs.Booking
{
    /// <summary>
    /// DTO for booking search parameters. All parameters are optional.
    /// </summary>
    public class BookingSearchRequestDto
    {
        public int? CustomerId { get; set; }
        public string? VehicleRegistrationNumber { get; set; }
        public int? MechanicId { get; set; }

        /// <summary>
        /// Search for bookings on or after this date (inclusive, date part).
        /// </summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// Search for bookings on or before this date (inclusive, date part).
        /// </summary>
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Specific status to filter by.
        /// </summary>
        public string? Status { get; set; } // Allow filtering by status string
    }
}