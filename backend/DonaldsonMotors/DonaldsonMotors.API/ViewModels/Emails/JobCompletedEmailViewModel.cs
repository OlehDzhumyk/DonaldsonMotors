namespace DonaldsonMotors.API.ViewModels.Emails
{
    public class JobCompletedEmailViewModel : BaseEmailViewModel
    {
        public int BookingId { get; set; }
        public string TotalAmountFormatted { get; set; } = string.Empty;
        public string PaymentUrl { get; set; } = string.Empty;
    }
}