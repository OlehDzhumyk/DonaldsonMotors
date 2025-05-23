using System.ComponentModel.DataAnnotations;

namespace DonaldsonMotors.API.Data.Entities
{
    public class Vehicle
    {
        [Key]
        public string RegistrationNumber { get; set; } = null!;  // PK
        public string Make { get; set; } = null!;
        public string Model { get; set; } = null!;
        public int Year { get; set; }
        public int Mileage { get; set; }

        // Ownership
        public int OwnerId { get; set; }
        public Customer Owner { get; set; } = null!;

        // Navigation
        public ICollection<Booking>? Bookings { get; set; }
    }
}