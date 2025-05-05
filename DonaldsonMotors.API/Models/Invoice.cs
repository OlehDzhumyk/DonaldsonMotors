namespace DonaldsonMotors.API.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public decimal TotalCost { get; set; }
        public DateTime DateIssued { get; set; }

        public Booking Booking { get; set; } = null!;
        public ICollection<Payment>? Payments { get; set; }
    }
}