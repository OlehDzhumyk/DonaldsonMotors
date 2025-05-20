namespace DonaldsonMotors.API.Data.Entities
{

    public class EmployeeEntity : ApplicationUser
    {
        public ICollection<JobEntity> JobsCompleted { get; set; } = new List<JobEntity>();
    }

}
