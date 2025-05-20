// Data/Entities/BookingEntity.cs
using System.ComponentModel.DataAnnotations;

namespace DonaldsonMotors.API.Data.Entities
{
    public class BookingEntity
    {
        [Key]
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string VehicleRegistration { get; set; } = null!;
        public DateTime BookingDate { get; set; }
        public string Status { get; set; } = null!;

        // Navigation
        public CustomerEntity Customer { get; set; } = null!;
        public VehicleEntity Vehicle { get; set; } = null!;
        public ICollection<JobEntity> Jobs { get; set; } = new List<JobEntity>();
        public InvoiceEntity? Invoice { get; set; }
    }
}
