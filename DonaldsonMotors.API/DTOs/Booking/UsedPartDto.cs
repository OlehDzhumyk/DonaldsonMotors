using System.ComponentModel.DataAnnotations;

namespace DonaldsonMotors.API.DTOs.Booking
{
    public class UsedPartDto
    {
        [Required]
        public int PartId { get; set; }
        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; }
    }

}
