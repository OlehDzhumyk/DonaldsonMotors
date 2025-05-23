using System.ComponentModel.DataAnnotations;

namespace DonaldsonMotors.API.DTOs.Booking
{
    public class FinishJobRequestDto
    {
        [Required]
        public string Description { get; set; } = string.Empty;
        [Required]
        [Range(0, 10000)]
        public decimal LabourCost { get; set; }
        public List<UsedPartDto> UsedParts { get; set; } = new();
    }

}
