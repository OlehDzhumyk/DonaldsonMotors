using DonaldsonMotors.API.Data.Entities;

namespace DonaldsonMotors.API.Data.Entities
{
    public class Booking
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int? MechanicId { get; set; }
        public string VehicleRegistrationNumber { get; set; } = null!;

        public DateTime SlotStart { get; set; }

        public BookingStatus Status { get; set; }

        public int ServiceTypeId { get; set; }
        public ServiceType ServiceType { get; set; } = null!;

        public DateTime? PaidAt { get; set; }
        public string? CancellationReason { get; set; } 
        public DateTime? CancelledAt { get; set; }
        public int? CancelledByUserId { get; set; }

        public virtual ApplicationUser? CancelledBy { get; set; }
        
        public virtual Employee? Mechanic { get; set; }

        public Customer Customer { get; set; } = null!;
        public Vehicle Vehicle { get; set; } = null!;
        public Invoice? Invoice { get; set; }
        public ICollection<Job>? Jobs { get; set; }



    }







}