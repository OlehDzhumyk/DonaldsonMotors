using System.ComponentModel.DataAnnotations;

namespace DonaldsonMotors.API.DTOs.Part
{
    /// <summary>
    /// DTO for updating an existing part. All fields are optional for partial updates.
    /// </summary>
    public class UpdatePartRequestDto
    {
        [StringLength(100, MinimumLength = 3)]
        public string? Name { get; set; }

        [Range(0.01, 10000.00)]
        public decimal? Price { get; set; }

        [Range(0.01, 10000.00)]
        public decimal? CostPrice { get; set; }

        [StringLength(50)]
        public string? Barcode { get; set; }

        public int? SupplierId { get; set; }
    }
}