namespace DonaldsonMotors.API.Data.Entities
{
    public class Part
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int CurrentStockLevel { get; set; }
        public int SupplierId { get; set; }

        public Supplier Supplier { get; set; } = null!;
        public ICollection<JobPart>? JobParts { get; set; }
    }

}