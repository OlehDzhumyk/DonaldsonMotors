
namespace DonaldsonMotors.API.Data.Entities
{
    public class JobPart
    {
        public int JobId { get; set; }
        public int PartId { get; set; }
        public int QuantityUsed { get; set; }

        /// <summary>
        /// The part's selling price when the mechanic used it. The invoice uses this,
        /// so later price changes don't alter jobs that are already finished.
        /// </summary>
        public decimal UnitPrice { get; set; }

        public Job Job { get; set; } = null!;
        public Part Part { get; set; } = null!;
    }

}