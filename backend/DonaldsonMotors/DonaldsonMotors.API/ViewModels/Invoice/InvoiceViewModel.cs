namespace DonaldsonMotors.API.ViewModels.Invoice
{
    public class InvoiceViewModel
    {
        public string InvoiceId { get; set; } = "N/A";
        public int BookingId { get; set; }
        public string DateIssuedFormatted { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;

        public string CompanyName { get; set; } = "Donaldson Motors";
        public string CompanyAddressLine1 { get; set; } = "123 Service Lane";
        public string? CompanyAddressLine2 { get; set; }
        public string CompanyPostcode { get; set; } = "Glasgow, G1 2AB";
        public string CompanyEmail { get; set; } = "contact@donaldsonmotors.com";
        public string CompanyPhone { get; set; } = "0141 123 4567";


        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerAddress { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }

        public string JobDescription { get; set; } = string.Empty;
        public string JobLabourCostFormatted { get; set; } = string.Empty;

        public List<InvoicePartItemViewModel> PartsUsed { get; set; } = new List<InvoicePartItemViewModel>();
        public string TotalPartsCostFormatted { get; set; } = string.Empty;
        public string GrandTotalFormatted { get; set; } = string.Empty;
        public int CurrentYear { get; set; } = DateTime.UtcNow.Year;
    }

    public class InvoicePartItemViewModel
    {
        public string PartName { get; set; } = string.Empty;
        public int QuantityUsed { get; set; }
        public string UnitPriceFormatted { get; set; } = string.Empty;
        public string SubtotalFormatted { get; set; } = string.Empty;
    }
}