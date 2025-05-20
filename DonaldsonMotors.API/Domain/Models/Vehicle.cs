// Domain/Models/Vehicle.cs
namespace DonaldsonMotors.API.Domain.Models
{
    public class Vehicle
    {
        public string Registration { get; set; } = null!;
        public string Make { get; set; } = null!;
        public string Model { get; set; } = null!;
        public int Year { get; set; }
        public int Mileage { get; set; }
    }
}
