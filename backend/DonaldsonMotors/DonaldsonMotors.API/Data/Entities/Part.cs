using System.Collections.Generic; // Required for ICollection
using System.ComponentModel.DataAnnotations.Schema; // Required for Column attribute

namespace DonaldsonMotors.API.Data.Entities
{
    public class Part
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

       
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

     
        [Column(TypeName = "decimal(18,2)")] 
        public decimal CostPrice { get; set; }

        public int CurrentStockLevel { get; set; }

        public string? Barcode { get; set; }

        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; } = null!;

        public ICollection<JobPart>? JobParts { get; set; }
    }
}