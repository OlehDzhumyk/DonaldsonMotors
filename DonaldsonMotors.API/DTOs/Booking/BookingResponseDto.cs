namespace DonaldsonMotors.API.DTOs.Booking
{
    public class BookingResponseDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string VehicleRegistration { get; set; } = null!;
        public DateTime BookingDate { get; set; }
        public string Status { get; set; } = null!;
    }
}
