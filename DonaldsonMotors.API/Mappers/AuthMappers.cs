// Mappers/AuthMappers.cs
using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Domain.Models;
using DonaldsonMotors.API.DTOs.Auth;

namespace DonaldsonMotors.API.Mappers
{
    public static class AuthMappers
    {
        /// <summary>
        /// Map a RegisterRequestDto to the EF Identity entity ApplicationUser.
        /// The role is typically stored in a separate Claim, but if you
        /// have a discriminator field on the entity (e.g. UserType), set it here.
        /// </summary>
        public static ApplicationUser ToEntity(this RegisterRequestDto dto)
        {
            return new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                // if you added a UserType or Role field on ApplicationUser:
                // UserType = dto.Role
            };
        }

        /// <summary>
        /// Map an EF ApplicationUser to a domain-level User (Customer or Employee).
        /// </summary>
        public static Domain.Models.User ToDomainModel(this ApplicationUser entity, string role)
        {
            if (role == Roles.Customer)
            {
                return new Customer
                {
                    Id = entity.Id,
                    FullName = entity.FullName!,
                    Email = entity.Email!,
                    Role = role,
                    Address = entity.Address,
                    TelephoneNumber = entity.TelephoneNumber
                };
            }
            else
            {
                return new Employee
                {
                    Id = entity.Id,
                    FullName = entity.FullName!,
                    Email = entity.Email!,
                    Role = role,
                    Address = entity.Address,
                    TelephoneNumber = entity.TelephoneNumber,
                    DateOfBirth = entity.DateOfBirth
                };
            }
        }

        /// <summary>
        /// Map LoginResponseDto or RegisterResponseDto (containing token + expiry) back to a domain auth result.
        /// Typically you don't need to map these, so this is just an example.
        /// </summary>
        public static AuthResult ToDomainResult(this LoginResponseDto dto)
        {
            return new AuthResult
            {
                Token = dto.Token,
                ExpiresAt = dto.ExpiresAt
            };
        }
    }

    /// <summary>
    /// Example domain model for returning authentication outcomes,
    /// if you need one in your Domain layer.
    /// </summary>
    public class AuthResult
    {
        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}
