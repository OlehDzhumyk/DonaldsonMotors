namespace DonaldsonMotors.API.DTOs.ServiceType
{
    public class ServiceTypeResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
    }
}
