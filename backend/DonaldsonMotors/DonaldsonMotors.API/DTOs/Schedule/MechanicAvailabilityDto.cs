namespace DonaldsonMotors.API.DTOs.Schedule
{
    /// <summary>
    /// Data Transfer Object representing a mechanic's availability for a specific time slot.
    /// </summary>
    public class MechanicAvailabilityDto
    {
        public int MechanicId { get; set; }
        public string MechanicName { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public string? ReasonIfNotAvailable { get; set; } // e.g., "Booked on another job"
    }
}