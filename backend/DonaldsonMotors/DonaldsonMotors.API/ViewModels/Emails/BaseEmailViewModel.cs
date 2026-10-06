namespace DonaldsonMotors.API.ViewModels.Emails
{
    public class BaseEmailViewModel
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = "Donaldson Motors";
        public int CurrentYear { get; set; } = DateTime.UtcNow.Year;
        public string CompanyContactEmail { get; set; } = "support@donaldsonmotors.com";
        public string WebsiteUrl { get; set; } = "http://localhost:5000";
    }
}