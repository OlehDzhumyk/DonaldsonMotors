using System.ComponentModel.DataAnnotations;

namespace DonaldsonMotors.API.Data.Entities
{
    /// <summary>
    /// Зберігає стандартні робочі години для кожного дня тижня.
    /// </summary>
    public class WorkingHours
    {
        [Key]
        public DayOfWeek DayOfWeek { get; set; }

        // Використовуємо TimeOnly для зберігання часу без дати
        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }
    }
}