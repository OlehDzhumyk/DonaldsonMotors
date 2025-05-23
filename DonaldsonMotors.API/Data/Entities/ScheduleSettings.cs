namespace DonaldsonMotors.API.Data.Entities
{
    /// <summary>
    /// Зберігає глобальні налаштування графіка, як-от обідня перерва.
    /// </summary>
    public class ScheduleSettings
    {
        // Використовуємо фіксований ID, щоб у таблиці завжди був один запис
        public int Id { get; set; } = 1;

        public TimeOnly LunchStartTime { get; set; }

        public TimeOnly LunchEndTime { get; set; }
    }
}