namespace DonaldsonMotors.API.Data.Entities
{
    public class Supplier
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string Postcode { get; set; } = null!;
        public string? Telephone { get; set; }
        public string? Email { get; set; }

        public ICollection<Part>? Parts { get; set; }
    }
}