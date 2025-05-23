namespace DonaldsonMotors.API.Data.Entities
{

    public class Employee : ApplicationUser
    {
        public ICollection<Job> JobsCompleted { get; set; } = new List<Job>();
    }

}
