namespace DonaldsonMotors.API.DTOs.Auth
{
    public class RegisterRequestDto
    {
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        // Ignored by the public /register endpoint, which always creates a Customer
        public string? Role { get; set; }
    }
}