using System.ComponentModel.DataAnnotations;

namespace DonaldsonMotors.API.DTOs.Supplier
{
    /// <summary>
    /// DTO for updating an existing supplier.
    /// All fields are optional for partial updates.
    /// </summary>
    public class UpdateSupplierRequestDto
    {
        [StringLength(100, MinimumLength = 2)]
        public string? Name { get; set; }

        [StringLength(100)]
        public string? AddressLine1 { get; set; }

        [StringLength(100)]
        public string? AddressLine2 { get; set; }

        [StringLength(10)]
        public string? Postcode { get; set; }

        [StringLength(20)]
        public string? Telephone { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }
    }
}