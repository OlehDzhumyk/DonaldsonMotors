    // DonaldsonMotors.API/Models/Vehicle.cs
namespace DonaldsonMotors.API.Models
{
    public class Vehicle
    {
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