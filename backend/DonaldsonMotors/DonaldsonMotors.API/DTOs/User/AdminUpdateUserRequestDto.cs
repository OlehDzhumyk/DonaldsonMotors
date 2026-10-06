namespace DonaldsonMotors.API.DTOs.User
{
    public class AdminUpdateUserRequestDto
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? TelephoneNumber { get; set; }
        public string? Email { get; set; } 
        public bool? IsEmailConfirmed { get; set; }
        public string? Role { get; set; } 
                                          
    }
}
