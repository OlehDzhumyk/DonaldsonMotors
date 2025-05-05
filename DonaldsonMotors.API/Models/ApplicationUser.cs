using Microsoft.AspNetCore.Identity;

namespace DonaldsonMotors.API.Models
{

    public abstract class ApplicationUser : IdentityUser<int>
    {

        public string FullName { get; set; } = null!;

        public DateTime? DateOfBirth { get; set; }
    }
}
