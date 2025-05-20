// Domain/Models/JobItem.cs
namespace DonaldsonMotors.API.Domain.Models
{
    public class JobItem
    {
        public Item Item { get; set; } = null!;
        public int QuantityUsed { get; set; }
    }
}
