namespace DonaldsonMotors.API.Models
{
    public class Job
    {
        public int Id { get; set; }
        public string Description { get; set; } = null!;
        public decimal LabourCost { get; set; }
        public decimal PartsCost { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public int TechnicianId { get; set; }
        public int BookingId { get; set; }

        public Employee Technician { get; set; } = null!;
        public Booking Booking { get; set; } = null!;
        public ICollection<JobItem>? JobItems { get; set; }
    }
}