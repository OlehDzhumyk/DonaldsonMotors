namespace DonaldsonMotors.API.DTOs.Vehicle
{
    public class UpdateVehicleRequestDto
    {
        public string? Make { get; set; }
        public string? Model { get; set; }
        public int? Year { get; set; }
        public int? Mileage { get; set; }
    }
}
