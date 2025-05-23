using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.DTOs.Auth;

namespace DonaldsonMotors.API.Mappers
{
    public static class AuthMapper
    {
        public static ApplicationUser ToApplicationUserEntity(this RegisterRequestDto dto)
        {
            ApplicationUser user = dto.Role == Roles.Customer
                ? new Customer()
                : new Employee(); // This assumes any non-customer role is an Employee type

            user.Email = dto.Email;
            user.UserName = dto.Email; // Important for Identity
            user.FullName = dto.FullName;

            return user;
        }
    }
}