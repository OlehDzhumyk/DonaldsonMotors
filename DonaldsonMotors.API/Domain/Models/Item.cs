// Domain/Models/Item.cs
namespace DonaldsonMotors.API.Domain.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int CurrentStock { get; set; }
        public Supplier Supplier { get; set; } = null!;
    }
}
