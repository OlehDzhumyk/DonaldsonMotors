using DonaldsonMotors.API.Data.Entities;
using System.Linq.Expressions;

namespace DonaldsonMotors.API.Interfaces
{
    /// <summary>
    /// A generic repository for common data access operations.
    /// All specific repositories should inherit from this.
    /// </summary>
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(int id);
        Task<IEnumerable<TEntity>> GetAllAsync();

        /// <summary>
        /// Finds entities based on a predicate (a condition).
        /// Example: FindAsync(c => c.Name == "John Doe")
        /// </summary>
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);

        Task AddAsync(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);
    }

    // --- Unit of Work ---

    /// <summary>
    /// Manages all repositories and saves changes to the database in a single transaction.
    /// Provides a single point of access to all repository interfaces.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IBookingRepository Bookings { get; }
        IInvoiceRepository Invoices { get; }
        IJobRepository Jobs { get; }
        IJobPartRepository JobParts { get; }
        IPartRepository Parts { get; }
        IPaymentRepository Payments { get; }
        ISupplierRepository Suppliers { get; }
        IUserRepository Users { get; }
        IVehicleRepository Vehicles { get; }
        IScheduleRepository Schedule { get; }
        IServiceTypeRepository ServiceTypes { get; }

        /// <summary>
        /// Saves all changes made in this unit of work to the database.
        /// </summary>
        /// <returns>The number of state entries written to the database.</returns>
        Task<int> CompleteAsync();
    }

    // --- Specific Repository Interfaces ---
    // They now inherit from IRepository and only contain their unique methods.

    public interface IBookingRepository : IRepository<Booking>
    {
        Task<Booking?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Booking>> GetBookingsByCustomerIdAsync(int customerId);
        Task<IEnumerable<Booking>> GetDashboardBookingsAsync();

        /// <summary>
        /// Searches for bookings based on various optional criteria.
        /// Includes related entities (Customer, Vehicle, ServiceType, Mechanic) for detailed DTO mapping.
        /// </summary>
        Task<IEnumerable<Booking>> SearchBookingsAsync(
            int? customerId,
            string? vehicleRegistration,
            int? mechanicId,
            DateTime? dateFrom,
            DateTime? dateTo,
            BookingStatus? status);

    }



    /// <summary>
    /// Centralized repository for all user-related data access,
    /// handling ApplicationUser, Customer, and Employee types due to TPH.
    /// Write operations for users (create, update password, roles) should generally be done via UserManager.
    /// </summary>
    public interface IUserRepository
    {
        // Generic User Methods (can return ApplicationUser, Customer, or Employee)
        Task<ApplicationUser?> GetUserByIdAsync(int id);
        Task<ApplicationUser?> GetUserByEmailAsync(string email);

        // Customer-Specific Methods
        Task<Customer?> GetCustomerByIdAsync(int id); // Gets specifically a Customer
        Task<Customer?> GetCustomerByIdWithVehiclesAsync(int id);

        // Employee-Specific Methods
        Task<Employee?> GetEmployeeByIdAsync(int id); // Gets specifically an Employee
        Task<IEnumerable<Employee>> GetAllEmployeesAsync(); // Gets all users who are Employees
        Task<IEnumerable<Employee>> GetEmployeesInRoleAsync(string roleName); // e.g., Get all Mechanics

        /// <summary>
        /// Gets all active bookings for a specific mechanic on a given date.
        /// Used by services to determine mechanic availability.
        /// </summary>
        Task<IEnumerable<Booking>> GetMechanicBookingsOnDateAsync(int mechanicId, DateTime date);
    }


    public interface IInvoiceRepository : IRepository<Invoice>
    {
        // No special methods needed at the moment.
    }

    public interface IJobRepository : IRepository<Job>
    {
        // No special methods needed at the moment.
    }

    public interface IPartRepository : IRepository<Part>
    {
        Task<Part?> GetByIdWithSupplierAsync(int id);
        Task<IEnumerable<Part>> GetAllWithSupplierAsync();
    }

    public interface IPaymentRepository : IRepository<Payment>
    {
        // No special methods needed at the moment.
    }

    public interface ISupplierRepository : IRepository<Supplier>
    {
        // No special methods needed at the moment.
    }


    public interface IServiceTypeRepository : IRepository<ServiceType>
    {
        // No special methods needed at the moment.
    }

    public interface IJobPartRepository : IRepository<JobPart>
    {
        // No special methods needed at the moment.
    }




    /// <summary>
    /// Repository for vehicles. The generic IRepository<T> uses an int PK, but Vehicle uses a string.
    /// </summary>
    public interface IVehicleRepository
    {
        Task<Vehicle?> GetByRegistrationAsync(string registration);
        Task<IEnumerable<Vehicle>> GetByOwnerIdAsync(int ownerId);
        Task AddAsync(Vehicle vehicle);
        void Update(Vehicle vehicle);
        void Delete(Vehicle vehicle);
    }

    /// <summary>
    /// A specialized repository for managing multiple schedule-related entities.
    /// It does not follow the generic pattern.
    /// </summary>
    public interface IScheduleRepository
    {
        // Methods for WorkingHours
        Task<IEnumerable<WorkingHours>> GetWorkingHoursAsync();
        Task UpdateWorkingHoursAsync(IEnumerable<WorkingHours> workingHours);

        // Methods for ScheduleSettings (lunch)
        Task<ScheduleSettings?> GetSettingsAsync();
        void UpdateSettings(ScheduleSettings settings);

        // Methods for ScheduleException
        Task<IEnumerable<ScheduleException>> GetExceptionsAsync();
        Task<ScheduleException?> GetExceptionByIdAsync(int id);
        Task AddExceptionAsync(ScheduleException exception);
        void DeleteException(ScheduleException exception);
    }
}