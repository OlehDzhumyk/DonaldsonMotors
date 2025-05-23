using System.ComponentModel.DataAnnotations;

namespace DonaldsonMotors.API.DTOs.Schedule
{
    /// <summary>
    /// DTO for defining working hours for a single day of the week.
    /// </summary>
    public class WorkingDayDto
    {
        [Required]
        public DayOfWeek DayOfWeek { get; set; }

        [Required]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Time must be in HH:mm format")]
        public string StartTime { get; set; } = null!;

        [Required]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Time must be in HH:mm format")]
        public string EndTime { get; set; } = null!;
    }



    /// <summary>
    /// DTO for defining the daily lunch break.
    /// </summary>
    public class LunchBreakDto
    {
        [Required]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Time must be in HH:mm format")]
        public string StartTime { get; set; } = null!;

        [Required]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Time must be in HH:mm format")]
        public string EndTime { get; set; } = null!;
    }


    /// <summary>
    /// DTO for creating a new schedule exception (e.g., a holiday).
    /// </summary>
    public class CreateScheduleExceptionDto
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Description { get; set; } = null!;
    }


    /// <summary>
    /// DTO for returning a schedule exception, including its ID.
    /// </summary>
    public class ScheduleExceptionResponseDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = null!;
    }

}