
namespace DonaldsonMotors.API.Data.Entities
{
    public class JobPart
    {
        public int JobId { get; set; }
        public int PartId { get; set; }
        public int QuantityUsed { get; set; }

        public Job Job { get; set; } = null!;
        public Part Part { get; set; } = null!;
    }

}