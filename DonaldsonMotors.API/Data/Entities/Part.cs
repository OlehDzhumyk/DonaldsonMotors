using System.Collections.Generic; // Required for ICollection
using System.ComponentModel.DataAnnotations.Schema; // Required for Column attribute

namespace DonaldsonMotors.API.Data.Entities
{
    public class Part
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        /// <summary>
        /// Price at which the part is sold to the customer.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")] // Ensure DB type is correct
        public decimal Price { get; set; }

        /// <summary>
        /// Cost of the part from the supplier.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")] // Ensure DB type is correct
        public decimal CostPrice { get; set; } // New field

        public int CurrentStockLevel { get; set; }

        public string? Barcode { get; set; } // New optional field

        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; } = null!;

        public ICollection<JobPart>? JobParts { get; set; }
    }
}