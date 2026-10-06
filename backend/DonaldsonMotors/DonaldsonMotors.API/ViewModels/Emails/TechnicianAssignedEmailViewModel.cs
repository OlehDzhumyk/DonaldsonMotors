namespace DonaldsonMotors.API.ViewModels.Emails
{
    public class TechnicianAssignedEmailViewModel : BaseEmailViewModel
    {
        public int BookingId { get; set; }
        public string MechanicName { get; set; } = string.Empty;
        public string BookingDateFormatted { get; set; } = string.Empty;
    }
}