namespace DonaldsonMotors.API.DTOs.ServiceType
{
    public class SlotResponseDto
    {
        public string Time { get; set; } = null!;  // e.g. "09:00"
        public bool Available { get; set; }
    }
}
