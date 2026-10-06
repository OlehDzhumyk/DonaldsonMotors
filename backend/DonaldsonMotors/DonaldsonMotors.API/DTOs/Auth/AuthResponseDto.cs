namespace DonaldsonMotors.API.DTOs.Auth
{
    /// <summary>
    /// Represents the successful result of a registration or login attempt.
    /// </summary>
    public class AuthResponseDto
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}