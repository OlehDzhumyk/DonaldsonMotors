using System.ComponentModel.DataAnnotations;

namespace DonaldsonMotors.API.DTOs.ServiceType
{
    /// <summary>
    /// DTO for updating an existing service type. All fields are optional.
    /// </summary>
    public class UpdateServiceTypeRequestDto
    {
        [StringLength(100, MinimumLength = 3)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0.01, 10000.00)]
        public decimal? Price { get; set; }

        [Range(0.5, 24.0)]
        public double? DurationHours { get; set; }
    }
}