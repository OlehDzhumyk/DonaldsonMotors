using System.ComponentModel.DataAnnotations;

namespace DonaldsonMotors.API.Data.Entities
{
    
    public class WorkingHours
    {
        [Key]
        public DayOfWeek DayOfWeek { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }
    }
}