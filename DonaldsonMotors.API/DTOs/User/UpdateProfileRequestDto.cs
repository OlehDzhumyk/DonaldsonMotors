// DTOs/User/UpdateProfileRequestDto.cs
namespace DonaldsonMotors.API.DTOs.User
{
    public class UpdateProfileRequestDto
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? TelephoneNumber { get; set; }
    }

}
