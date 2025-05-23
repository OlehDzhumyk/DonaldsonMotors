namespace DonaldsonMotors.API.Data.Entities
{
    public class Customer : ApplicationUser
    {
        public List<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public List<Booking> Bookings { get; set; } = new List<Booking>();
    }


}