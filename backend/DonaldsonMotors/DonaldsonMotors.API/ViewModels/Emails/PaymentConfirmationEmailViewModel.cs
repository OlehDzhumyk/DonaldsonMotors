namespace DonaldsonMotors.API.ViewModels.Emails
{
    public class PaymentConfirmationEmailViewModel : BaseEmailViewModel
    {
        public int BookingId { get; set; }
        public string AmountPaidFormatted { get; set; } = string.Empty;
        public string DatePaidFormatted { get; set; } = string.Empty;
        public string InvoiceDownloadUrl { get; set; } = string.Empty;
    }
}