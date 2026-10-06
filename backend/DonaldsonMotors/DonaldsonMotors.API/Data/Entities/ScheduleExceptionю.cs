namespace DonaldsonMotors.API.Data.Entities
{
    
    public class ScheduleException
    {
        public int Id { get; set; }

        public DateOnly Date { get; set; }

        public string Description { get; set; } = string.Empty;
    }
}