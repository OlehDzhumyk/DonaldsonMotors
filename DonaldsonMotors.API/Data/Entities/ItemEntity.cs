// Data/Entities/ItemEntity.cs
using System.ComponentModel.DataAnnotations;

namespace DonaldsonMotors.API.Data.Entities
{
    public class ItemEntity
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int CurrentStock { get; set; }

        public int SupplierId { get; set; }
        public SupplierEntity Supplier { get; set; } = null!;
        public ICollection<JobItemEntity> JobItems { get; set; } = new List<JobItemEntity>();
    }
}
