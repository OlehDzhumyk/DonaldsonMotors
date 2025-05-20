namespace DonaldsonMotors.API.DTOs.Booking
{
    public class UpdateBookingRequestDto
    {
        public DateTime? BookingDate { get; set; }
        public string? Status { get; set; }
        public string? VehicleRegistration { get; set; }
    }
}
