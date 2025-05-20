// Data/Entities/InvoiceEntity.cs
using System.ComponentModel.DataAnnotations;

namespace DonaldsonMotors.API.Data.Entities
{
    public class InvoiceEntity
    {
        [Key]
        public int Id { get; set; }
        public int BookingId { get; set; }
        public decimal TotalCost { get; set; }
        public DateTime DateIssued { get; set; }

        public BookingEntity Booking { get; set; } = null!;
        public ICollection<PaymentEntity> Payments { get; set; } = new List<PaymentEntity>();
    }
}
