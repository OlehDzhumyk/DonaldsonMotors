using System.ComponentModel.DataAnnotations;

namespace DonaldsonMotors.API.DTOs.Part
{
    /// <summary>
    /// DTO for updating the stock level of a part.
    /// </summary>
    public class UpdateStockLevelRequestDto
    {
        [Required]
        // Can be positive (for new stock) or negative (for manual corrections).
        // For sales, stock is reduced via FinishJob.
        public int ChangeInQuantity { get; set; }

        [StringLength(200)]
        public string? Reason { get; set; } // Optional reason for change, e.g., "Stock Intake INV-001", "Correction - Damaged"
    }
}