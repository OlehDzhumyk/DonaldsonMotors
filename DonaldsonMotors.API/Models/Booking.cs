namespace DonaldsonMotors.API.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string VehicleRegistration { get; set; } = null!;
        public DateTime BookingDate { get; set; }
        public string Status { get; set; } = null!;

        public Customer User { get; set; } = null!;
        public Vehicle Vehicle { get; set; } = null!;
        public Invoice? Invoice { get; set; }
        public ICollection<Job>? Jobs { get; set; }
    }
}