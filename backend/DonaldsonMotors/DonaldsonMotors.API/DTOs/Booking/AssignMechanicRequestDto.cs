using System.ComponentModel.DataAnnotations;

namespace DonaldsonMotors.API.DTOs.Booking
{
    public class AssignMechanicRequestDto
    {
        [Required]
        public int BookingId { get; set; }
        [Required]
        public int MechanicId { get; set; }
    }

}
