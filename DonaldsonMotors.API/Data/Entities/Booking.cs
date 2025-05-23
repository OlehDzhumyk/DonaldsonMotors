using DonaldsonMotors.API.Data.Entities;

namespace DonaldsonMotors.API.Data.Entities
{
    public class Booking
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int? MechanicId { get; set; }
        public string VehicleRegistrationNumber { get; set; } = null!;

        // Змінено: тепер це конкретний час початку 2-годинного слоту
        public DateTime SlotStart { get; set; }

        public BookingStatus Status { get; set; }

        // Додано: зв'язок з типом послуги для визначення деталей роботи
        public int ServiceTypeId { get; set; }
        public ServiceType ServiceType { get; set; } = null!;

        // Навігаційні властивості
        public Customer Customer { get; set; } = null!;
        public Vehicle Vehicle { get; set; } = null!;
        public Invoice? Invoice { get; set; }
        public ICollection<Job>? Jobs { get; set; }
    }
}