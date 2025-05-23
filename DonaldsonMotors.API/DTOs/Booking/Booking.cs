using System.ComponentModel.DataAnnotations;

namespace DonaldsonMotors.API.DTOs.Booking
{


    public class UpdateBookingRequestDto
    {
        public DateTime? BookingDate { get; set; }
        public string? ServiceDescription { get; set; }
    }

    public class JobResponseDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = null!;
        public decimal LabourCost { get; set; }
        public decimal PartsCost { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public int MechanicId { get; set; }
        public string? MechanicName { get; set; }
        public IEnumerable<JobPartResponseDto>? UsedParts { get; set; }
    }

    public class JobPartResponseDto
    {
        public int PartId { get; set; }
        public string PartName { get; set; } = null!;
        public int QuantityUsed { get; set; }
        public decimal PricePerUnit { get; set; }
    }

    public class CompleteJobRequestDto
    {
        public decimal LabourCost { get; set; }
        public List<UsedPartDto> UsedParts { get; set; } = new();
    }


}
