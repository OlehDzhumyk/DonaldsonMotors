// Data/Entities/JobItemEntity.cs
namespace DonaldsonMotors.API.Data.Entities
{
    public class JobItemEntity
    {
        public int JobEntityId { get; set; }
        public JobEntity Job { get; set; } = null!;

        public int ItemEntityId { get; set; }
        public ItemEntity Item { get; set; } = null!;

        public int QuantityUsed { get; set; }
    }
}
