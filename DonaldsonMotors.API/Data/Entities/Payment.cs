namespace DonaldsonMotors.API.Data.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public string Method { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime DatePaid { get; set; }

        public Invoice Invoice { get; set; } = null!;
    }
}