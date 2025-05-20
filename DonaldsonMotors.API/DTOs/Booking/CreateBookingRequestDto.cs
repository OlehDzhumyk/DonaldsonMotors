namespace DonaldsonMotors.API.DTOs.Booking
{
    public class CreateBookingRequestDto
    {
        public int ServiceTypeId { get; set; }
        public string VehicleRegistration { get; set; } = null!;
        public DateTime BookingDate { get; set; }
        public string? Notes { get; set; }
    }
}
