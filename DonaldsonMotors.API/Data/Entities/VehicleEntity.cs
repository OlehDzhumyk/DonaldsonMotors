// Data/Entities/VehicleEntity.cs
using System.ComponentModel.DataAnnotations;

namespace DonaldsonMotors.API.Data.Entities
{
    public class VehicleEntity
    {
        [Key]
        public string RegistrationNumber { get; set; } = null!;
        public string Make { get; set; } = null!;
        public string Model { get; set; } = null!;
        public int Year { get; set; }
        public int Mileage { get; set; }

        // FK → CustomerEntity
        public int CustomerId { get; set; }
        public CustomerEntity Customer { get; set; } = null!;
    }
}
