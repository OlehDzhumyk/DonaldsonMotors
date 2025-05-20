using DonaldsonMotors.API.DTOs.Vehicle;

namespace DonaldsonMotors.API.DTOs.Customer
{
    public class CustomerProfileResponseDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Address { get; set; }
        public string? TelephoneNumber { get; set; }
        public IEnumerable<VehicleResponseDto> Vehicles { get; set; } = new List<VehicleResponseDto>();
    }
}