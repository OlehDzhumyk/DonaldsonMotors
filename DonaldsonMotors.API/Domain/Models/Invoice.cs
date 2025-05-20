// Domain/Models/Invoice.cs
namespace DonaldsonMotors.API.Domain.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public Booking Booking { get; set; } = null!;
        public decimal TotalCost { get; set; }
        public DateTime DateIssued { get; set; }
        public List<Payment> Payments { get; set; } = new();
    }
}
