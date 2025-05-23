// DTOs/User/UserProfileResponseDto.cs
using DonaldsonMotors.API.DTOs.Vehicle;

namespace DonaldsonMotors.API.DTOs.User
{
    public class UserProfileResponseDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public List<VehicleResponseDto> Vehicles { get; set; } = new();
    }

}
