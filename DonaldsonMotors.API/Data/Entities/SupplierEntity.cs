// Data/Entities/SupplierEntity.cs
using System.ComponentModel.DataAnnotations;

namespace DonaldsonMotors.API.Data.Entities
{
    public class SupplierEntity
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string AddressLine1 { get; set; } = null!;
        public string? AddressLine2 { get; set; }
        public string Postcode { get; set; } = null!;
        public string Telephone { get; set; } = null!;
        public string EmailContact { get; set; } = null!;

        public ICollection<ItemEntity> Items { get; set; } = new List<ItemEntity>();
    }
}
