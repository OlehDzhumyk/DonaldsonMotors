// Domain/Models/Supplier.cs
namespace DonaldsonMotors.API.Domain.Models
{
    public class Supplier
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string AddressLine1 { get; set; } = null!;
        public string? AddressLine2 { get; set; }
        public string Postcode { get; set; } = null!;
        public string Telephone { get; set; } = null!;
        public string EmailContact { get; set; } = null!;
    }
}
    