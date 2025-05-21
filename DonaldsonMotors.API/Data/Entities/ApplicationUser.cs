using Microsoft.AspNetCore.Identity;

namespace DonaldsonMotors.API.Data.Entities
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FullName { get; set; } = null!;
        public string? Address { get; set; } = null!;
        public string? TelephoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }

    }
}