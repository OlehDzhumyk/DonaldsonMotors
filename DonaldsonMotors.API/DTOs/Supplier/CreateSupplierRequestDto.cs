using System.ComponentModel.DataAnnotations;

namespace DonaldsonMotors.API.DTOs.Supplier
{
    /// <summary>
    /// DTO for creating a new supplier.
    /// </summary>
    public class CreateSupplierRequestDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? AddressLine1 { get; set; }

        [StringLength(100)]
        public string? AddressLine2 { get; set; }

        [Required]
        [StringLength(10)]
        public string Postcode { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Telephone { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }
    }
}