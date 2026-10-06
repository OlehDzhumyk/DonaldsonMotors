namespace DonaldsonMotors.API.DTOs.Booking
{
    public class MarkAsPaidRequestDto
    {
        public string? PaymentNotes { get; set; } // e.g., "Paid in cash", "Card ending 1234"
    }
}