using System.ComponentModel.DataAnnotations;

namespace DonaldsonMotors.API.Data.Entities
{
    public class Invoice
    {
        [Key]
        public int Id { get; set; }
        public int BookingId { get; set; }
        public decimal TotalCost { get; set; }
        public DateTime DateIssued { get; set; }

        public Booking Booking { get; set; } = null!;
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
