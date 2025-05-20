namespace DonaldsonMotors.API.DTOs.Vehicle
{
    public class CreateVehicleRequestDto
    {
        public string RegistrationNumber { get; set; } = null!;
        public string Make { get; set; } = null!;
        public string Model { get; set; } = null!;
        public int Year { get; set; }
        public int Mileage { get; set; }
    }
}
