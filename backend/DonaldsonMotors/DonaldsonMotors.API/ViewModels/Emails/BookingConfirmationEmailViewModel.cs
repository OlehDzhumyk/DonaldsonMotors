namespace DonaldsonMotors.API.ViewModels.Emails
{
    public class BookingConfirmationEmailViewModel : BaseEmailViewModel
    {
        public int BookingId { get; set; }
        public string ServiceTypeName { get; set; } = string.Empty;
        public string VehicleRegistration { get; set; } = string.Empty;
        public string BookingDateFormatted { get; set; } = string.Empty;
    }
}