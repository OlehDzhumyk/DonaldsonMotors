namespace DonaldsonMotors.API.DTOs.Part
{
    /// <summary>
    /// DTO for returning part details.
    /// </summary>
    public class PartResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal CostPrice { get; set; }
        public int CurrentStockLevel { get; set; }
        public string? Barcode { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty; // Include supplier name for convenience
    }
}