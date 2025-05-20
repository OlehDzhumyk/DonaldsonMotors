// Data/Entities/CustomerEntity.cs
using Microsoft.AspNetCore.Identity;

namespace DonaldsonMotors.API.Data.Entities
{
    public class CustomerEntity : ApplicationUser
    {
        public ICollection<VehicleEntity> Vehicles { get; set; } = new List<VehicleEntity>();
        public ICollection<BookingEntity> Bookings { get; set; } = new List<BookingEntity>();
    }


}
