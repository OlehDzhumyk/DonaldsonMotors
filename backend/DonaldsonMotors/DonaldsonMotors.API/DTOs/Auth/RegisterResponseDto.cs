namespace DonaldsonMotors.API.DTOs.Auth
{
    public class RegisterResponseDto
    {
        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}
