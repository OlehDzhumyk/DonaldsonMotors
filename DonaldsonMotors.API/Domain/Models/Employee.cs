namespace DonaldsonMotors.API.Domain.Models
{
    public class Employee : User
    {
        public DateTime? DateOfBirth { get; set; }
        public List<Job> JobsCompleted { get; set; } = new List<Job>();
    }


}