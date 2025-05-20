using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Domain.Models;

namespace DonaldsonMotors.API.Mappers
{
    public static class UserMapper
    {

        // From Entity to Domain Model
        public static Employee ToDomainModel(this EmployeeEntity entity)
        {
            return new Employee
            {
                Id = entity.Id,
                FullName = entity.FullName,
                Email = entity.Email,
                Role = "Employee", // Role set based on entity type
                Address = entity.Address,
                TelephoneNumber = entity.TelephoneNumber,
                DateOfBirth = entity.DateOfBirth,
                JobsCompleted = entity.JobsCompleted?.Select(j => j.ToDomainModel()).ToList() ?? new List<Job>()
            };
        }

        public static Customer ToDomainModel(this CustomerEntity entity)
        {
            return new Customer
            {
                Id = entity.Id,
                FullName = entity.FullName,
                Email = entity.Email,
                Role = "Customer", // Role set based on entity type
                Address = entity.Address,
                TelephoneNumber = entity.TelephoneNumber,
                Vehicles = entity.Vehicles?.Select(v => v.ToDomainModel()).ToList() ?? new List<Vehicle>(),
                Bookings = entity.Bookings?.Select(b => b.ToDomainModel()).ToList() ?? new List<Booking>()
            };
        }

        // From Domain Model to Entity

        public static EmployeeEntity ToEntity(this Employee employee)
        {
            return new EmployeeEntity
            {
                Id = employee.Id,
                UserName = employee.Email, // UserName set to Email, common in Identity
                Email = employee.Email,
                FullName = employee.FullName,
                Address = employee.Address,
                TelephoneNumber = employee.TelephoneNumber,
                DateOfBirth = employee.DateOfBirth,
                JobsCompleted = employee.JobsCompleted?.Select(j => j.ToEntity()).ToList() ?? new List<JobEntity>()
            };
        }

        public static CustomerEntity ToEntity(this Customer customer)
        {
            return new CustomerEntity
            {
                Id = customer.Id,
                UserName = customer.Email, // UserName set to Email, common in Identity
                Email = customer.Email,
                FullName = customer.FullName,
                Address = customer.Address,
                TelephoneNumber = customer.TelephoneNumber,
                Vehicles = customer.Vehicles?.Select(v => v.ToEntity(customer.Id)).ToList() ?? new List<VehicleEntity>(),
                Bookings = customer.Bookings?.Select(b => b.ToEntity()).ToList() ?? new List<BookingEntity>()
            };
        }

    }

}