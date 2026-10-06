namespace DonaldsonMotors.API.Data.Entities
{
    
    public class ScheduleSettings
    {
        public int Id { get; set; } = 1;

        public TimeOnly LunchStartTime { get; set; }

        public TimeOnly LunchEndTime { get; set; }
    }
}