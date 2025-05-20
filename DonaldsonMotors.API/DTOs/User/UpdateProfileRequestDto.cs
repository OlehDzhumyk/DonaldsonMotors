namespace DonaldsonMotors.API.DTOs.User
{
    public class UpdateProfileRequestDto
    {
        public string FullName { get; set; } = null!;
        public string? Address { get; set; }
        public string? TelephoneNumber { get; set; }
    }
}
