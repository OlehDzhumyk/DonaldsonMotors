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
                : new Employee();

            user.Email = dto.Email;
            user.UserName = dto.Email;
            user.FullName = dto.FullName;

            return user;
        }
    }
}