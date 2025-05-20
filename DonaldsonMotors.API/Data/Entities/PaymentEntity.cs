// Data/Entities/PaymentEntity.cs
using System.ComponentModel.DataAnnotations;

namespace DonaldsonMotors.API.Data.Entities
{
    public class PaymentEntity
    {
        [Key]
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public string Method { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime DatePaid { get; set; }

        public InvoiceEntity Invoice { get; set; } = null!;
    }
}
