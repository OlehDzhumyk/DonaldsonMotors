// DTOs/Booking/CreateBookingRequestDto.cs
using System.ComponentModel.DataAnnotations;

public class CreateBookingRequestDto
{
    [Required]
    public string VehicleRegistrationNumber { get; set; } = null!;

    [Required]
    public int ServiceTypeId { get; set; }

    [Required]
    public DateTime SlotStart { get; set; }
}
