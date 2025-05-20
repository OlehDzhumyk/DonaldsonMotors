// Domain/Models/Booking.cs
namespace DonaldsonMotors.API.Domain.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public Vehicle Vehicle { get; set; } = null!;
        public Customer Customer { get; set; } = null!;
        public DateTime BookingDate { get; set; }
        public string Status { get; set; } = null!;
        public List<Job> Jobs { get; set; } = new();
        public Invoice? Invoice { get; set; }
    }
}
