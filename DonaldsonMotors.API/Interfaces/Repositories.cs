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
        ICustomerRepository Customers { get; }
        IEmployeeRepository Employees { get; }
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
    }

    public interface ICustomerRepository : IRepository<Customer>
    {
        // Add this method to get a customer with their vehicles included.
        Task<Customer?> GetByIdWithVehiclesAsync(int id);
    }

    public interface IEmployeeRepository : IRepository<Employee>
    {
        // This is a special method unique to employees (mechanics)
        Task<bool> IsMechanicAvailableAsync(int mechanicId, DateTime slotStart, double durationHours);
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
        // No special methods needed at the moment.
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
    /// Repository for users. Doesn't use the generic IRepository because IdentityUser's PK is managed differently.
    /// </summary>
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetByIdAsync(int id);
        Task<ApplicationUser?> GetByEmailAsync(string email);
        // Add, Update, Delete are handled by UserManager from ASP.NET Core Identity.
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