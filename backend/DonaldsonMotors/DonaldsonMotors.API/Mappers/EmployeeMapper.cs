using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.DTOs.Schedule;

namespace DonaldsonMotors.API.Mappers
{
    public static class EmployeeMapper
    {
        /// <summary>
        /// Maps an Employee entity to a MechanicAvailabilityDto.
        /// The availability status and reason are passed in as they are determined by service logic.
        /// </summary>
        public static MechanicAvailabilityDto ToMechanicAvailabilityDto(
            this Employee employee,
            bool isAvailable,
            string? reasonIfNotAvailable = null)
        {
            if (employee == null) return null!;

            return new MechanicAvailabilityDto
            {
                MechanicId = employee.Id,
                MechanicName = employee.FullName,
                IsAvailable = isAvailable,
                ReasonIfNotAvailable = isAvailable ? null : reasonIfNotAvailable
            };
        }
    }
}