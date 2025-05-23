namespace DonaldsonMotors.API.Data.Entities
{
    public class Job
    {
        public int Id { get; set; }
        public string Description { get; set; } = null!;
        public decimal LabourCost { get; set; }
        public decimal PartsCost { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }

        // Вказуємо механіка, що виконував роботу
        public int MechanicId { get; set; }

        public int BookingId { get; set; }

        public Employee Mechanic { get; set; } = null!;
        public Booking Booking { get; set; } = null!;

        public ICollection<JobPart>? JobParts { get; set; }
    }
}