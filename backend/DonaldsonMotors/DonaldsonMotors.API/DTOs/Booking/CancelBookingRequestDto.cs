using System.ComponentModel.DataAnnotations;

namespace DonaldsonMotors.API.DTOs.Booking
{
    public class CancelBookingRequestDto
    {
        [Required]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Cancellation reason must be between 10 and 500 characters.")]
        public string Reason { get; set; } = string.Empty;
    }
}