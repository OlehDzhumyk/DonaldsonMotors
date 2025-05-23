namespace DonaldsonMotors.API.Data.Entities
{
    /// <summary>
    /// Зберігає винятки з робочого графіка, наприклад, свята.
    /// </summary>
    public class ScheduleException
    {
        public int Id { get; set; }

        // Використовуємо DateOnly для зберігання дати без часу
        public DateOnly Date { get; set; }

        public string Description { get; set; } = string.Empty;
    }
}