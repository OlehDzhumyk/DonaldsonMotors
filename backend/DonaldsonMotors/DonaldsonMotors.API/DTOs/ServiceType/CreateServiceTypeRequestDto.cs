using System.ComponentModel.DataAnnotations;

namespace DonaldsonMotors.API.DTOs.ServiceType
{
    /// <summary>
    /// DTO for creating a new service type.
    /// </summary>
    public class CreateServiceTypeRequestDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 10000.00)]
        public decimal Price { get; set; }

        [Required]
        [Range(0.5, 24.0)] // Example: from 30 mins to 24 hours
        public double DurationHours { get; set; } = 2.0;
    }
}