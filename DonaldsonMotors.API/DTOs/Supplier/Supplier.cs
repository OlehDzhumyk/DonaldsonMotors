namespace DonaldsonMotors.API.DTOs.Supplier
{
    /// <summary>
    /// DTO for returning supplier details.
    /// </summary>
    public class SupplierResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string Postcode { get; set; } = string.Empty;
        public string? Telephone { get; set; }
        public string? Email { get; set; }
        // We might not always want to return all parts for a supplier here
        // public List<PartResponseDto> Parts { get; set; } = new(); 
    }


}
