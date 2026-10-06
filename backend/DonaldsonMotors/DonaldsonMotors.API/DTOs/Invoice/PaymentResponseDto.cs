namespace DonaldsonMotors.API.DTOs.Invoice
{
    public class PaymentResponseDto
    {
        public int Id { get; set; }
        public string Method { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime DatePaid { get; set; }
    }
}
