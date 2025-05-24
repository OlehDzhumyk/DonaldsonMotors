using System.ComponentModel.DataAnnotations;

namespace DonaldsonMotors.API.DTOs.Part
{
    /// <summary>
    /// DTO for creating a new part.
    /// </summary>
    public class CreatePartRequestDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 10000.00)]
        public decimal Price { get; set; }

        [Required]
        [Range(0.01, 10000.00)]
        public decimal CostPrice { get; set; }

        [Required]
        [Range(0, 10000)]
        public int InitialStockLevel { get; set; }

        [StringLength(50)]
        public string? Barcode { get; set; }

        [Required]
        public int SupplierId { get; set; }
    }
}