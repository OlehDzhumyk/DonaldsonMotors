namespace DonaldsonMotors.API.Domain.Models
{
    public abstract class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string? TelephoneNumber { get; set; }
    }

}
