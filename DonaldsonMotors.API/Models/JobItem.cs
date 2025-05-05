namespace DonaldsonMotors.API.Models
{
    public class JobItem
    {
        public int JobId { get; set; }
        public int ItemId { get; set; }
        public int QuantityUsed { get; set; }

        public Job Job { get; set; } = null!;
        public Item Item { get; set; } = null!;
    }
}