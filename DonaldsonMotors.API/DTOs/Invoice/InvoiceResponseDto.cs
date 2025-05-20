namespace DonaldsonMotors.API.DTOs.Invoice
{
    public class InvoiceResponseDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public decimal TotalCost { get; set; }
        public DateTime DateIssued { get; set; }
    }
}
