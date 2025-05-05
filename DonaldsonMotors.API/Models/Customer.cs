using System.Collections.Generic;

namespace DonaldsonMotors.API.Models
{
    public class Customer : ApplicationUser
    {
        public string? Address { get; set; }
        public string? TelephoneNumber { get; set; }

        // Navigation
        public ICollection<Vehicle>? Vehicles { get; set; }
        public ICollection<Booking>? Bookings { get; set; }
        public ICollection<Invoice>? Invoices { get; set; }
    }
}