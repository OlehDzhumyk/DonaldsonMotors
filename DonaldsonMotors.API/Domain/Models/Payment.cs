// Domain/Models/Payment.cs
namespace DonaldsonMotors.API.Domain.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public Invoice Invoice { get; set; } = null!;
        public string Method { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime DatePaid { get; set; }
    }
}
