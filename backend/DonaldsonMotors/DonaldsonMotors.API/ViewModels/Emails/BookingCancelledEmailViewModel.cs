namespace DonaldsonMotors.API.ViewModels.Emails
{
    public class BookingCancelledEmailViewModel : BaseEmailViewModel
    {
        public int BookingId { get; set; }
        public string BookingDateFormatted { get; set; } = string.Empty;
        public string? CancellationReason { get; set; }
    }
}