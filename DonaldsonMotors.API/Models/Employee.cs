using System.Collections.Generic;

namespace DonaldsonMotors.API.Models
{

    public class Employee : ApplicationUser
    {
        public string Role { get; set; } = null!;  // use Roles constants

        // Navigation
        public ICollection<Job>? JobsCompleted { get; set; }
    }
}