// DTOs/Booking/BookingResponseDto.cs
namespace DonaldsonMotors.API.DTOs.Booking
{
    public class BookingResponseDto
    {
        public int Id { get; set; }
        public DateTime SlotStart { get; set; }
        public string Status { get; set; } = null!;
        public string VehicleRegistrationNumber { get; set; } = null!;
        public string ServiceTypeName { get; set; } = null!;
        public decimal ServiceTypePrice { get; set; }
    }
}
