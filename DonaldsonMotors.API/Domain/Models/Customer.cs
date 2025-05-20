// Domain/Models/Customer.cs
namespace DonaldsonMotors.API.Domain.Models
{
    public class Customer : User
    {
        public List<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public List<Booking> Bookings { get; set; } = new List<Booking>();
    }


}
