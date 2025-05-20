// Data/Entities/JobEntity.cs
using System.ComponentModel.DataAnnotations;

namespace DonaldsonMotors.API.Data.Entities
{
    public class JobEntity
    {
        [Key]
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int TechnicianId { get; set; }
        public string Description { get; set; } = null!;
        public decimal LabourCost { get; set; }
        public decimal PartsCost { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }

        public BookingEntity Booking { get; set; } = null!;
        public EmployeeEntity Technician { get; set; } = null!;
        // many-to-many via join table
        public ICollection<JobItemEntity> Items { get; set; } = new List<JobItemEntity>();
    }
}
