using System.Linq.Expressions;
using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Interfaces;
using Moq;

namespace DonaldsonMotors.Tests.Unit;

/// <summary>
/// An IUnitOfWork made of Moq repositories backed by in-memory lists,
/// so service tests can arrange data and check what was saved.
/// </summary>
public sealed class FakeUnitOfWork
{
    public List<Booking> Bookings { get; } = [];
    public List<Part> Parts { get; } = [];
    public List<ServiceType> ServiceTypes { get; } = [];
    public List<Job> Jobs { get; } = [];
    public List<Invoice> Invoices { get; } = [];
    public List<Payment> Payments { get; } = [];
    public List<JobPart> JobParts { get; } = [];
    public List<Customer> Customers { get; } = [];
    public List<Employee> Employees { get; } = [];
    public List<WorkingHours> WorkingHours { get; } = [];
    public List<ScheduleException> ScheduleExceptions { get; } = [];
    public ScheduleSettings Settings { get; } = new() { Id = 1, LunchStartTime = new TimeOnly(13, 0), LunchEndTime = new TimeOnly(14, 0) };

    public Mock<IUnitOfWork> Mock { get; } = new();
    public IUnitOfWork Object => Mock.Object;
    public int SaveCount { get; private set; }

    public FakeUnitOfWork()
    {
        var bookings = RepositoryOver<IBookingRepository, Booking>(Bookings, b => b.Id);
        bookings.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => Bookings.SingleOrDefault(b => b.Id == id));
        Mock.Setup(u => u.Bookings).Returns(bookings.Object);

        var parts = RepositoryOver<IPartRepository, Part>(Parts, p => p.Id);
        Mock.Setup(u => u.Parts).Returns(parts.Object);
        Mock.Setup(u => u.ServiceTypes).Returns(RepositoryOver<IServiceTypeRepository, ServiceType>(ServiceTypes, s => s.Id).Object);
        Mock.Setup(u => u.Jobs).Returns(RepositoryOver<IJobRepository, Job>(Jobs, j => j.Id).Object);
        Mock.Setup(u => u.Invoices).Returns(RepositoryOver<IInvoiceRepository, Invoice>(Invoices, i => i.Id).Object);
        Mock.Setup(u => u.Payments).Returns(RepositoryOver<IPaymentRepository, Payment>(Payments, p => p.Id).Object);
        Mock.Setup(u => u.JobParts).Returns(RepositoryOver<IJobPartRepository, JobPart>(JobParts, _ => 0).Object);

        var users = new Mock<IUserRepository>();
        users.Setup(r => r.GetCustomerByIdWithVehiclesAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => Customers.SingleOrDefault(c => c.Id == id));
        users.Setup(r => r.GetEmployeeByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => Employees.SingleOrDefault(e => e.Id == id));
        users.Setup(r => r.GetUserByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => (ApplicationUser?)Customers.SingleOrDefault(c => c.Id == id) ?? Employees.SingleOrDefault(e => e.Id == id));
        users.Setup(r => r.GetMechanicBookingsOnDateAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync((int mechanicId, DateTime date) => Bookings.Where(b =>
                b.MechanicId == mechanicId && b.SlotStart.Date == date.Date && b.Status != BookingStatus.Cancelled).ToList());
        Mock.Setup(u => u.Users).Returns(users.Object);

        var vehicles = new Mock<IVehicleRepository>();
        vehicles.Setup(r => r.GetByRegistrationAsync(It.IsAny<string>()))
            .ReturnsAsync((string reg) => Customers.SelectMany(c => c.Vehicles).SingleOrDefault(v => v.RegistrationNumber == reg));
        Mock.Setup(u => u.Vehicles).Returns(vehicles.Object);

        var schedule = new Mock<IScheduleRepository>();
        schedule.Setup(r => r.GetWorkingHoursAsync()).ReturnsAsync(() => WorkingHours);
        schedule.Setup(r => r.GetSettingsAsync()).ReturnsAsync(() => Settings);
        schedule.Setup(r => r.GetExceptionsAsync()).ReturnsAsync(() => ScheduleExceptions);
        Mock.Setup(u => u.Schedule).Returns(schedule.Object);

        Mock.Setup(u => u.CompleteAsync()).ReturnsAsync(() => ++SaveCount);
    }

    /// <summary>A repository whose reads and adds go to <paramref name="items"/>; new items get the next id.</summary>
    private static Mock<TRepo> RepositoryOver<TRepo, TEntity>(List<TEntity> items, Func<TEntity, int> getId)
        where TRepo : class, IRepository<TEntity>
        where TEntity : class
    {
        var repo = new Mock<TRepo>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => items.SingleOrDefault(i => getId(i) == id));
        repo.Setup(r => r.GetAllAsync()).ReturnsAsync(() => items.ToList());
        repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<TEntity, bool>>>()))
            .ReturnsAsync((Expression<Func<TEntity, bool>> predicate) => items.Where(predicate.Compile()).ToList());
        repo.Setup(r => r.AddAsync(It.IsAny<TEntity>()))
            .Callback((TEntity entity) =>
            {
                var idProperty = typeof(TEntity).GetProperty("Id");
                if (idProperty?.PropertyType == typeof(int) && (int)idProperty.GetValue(entity)! == 0)
                {
                    idProperty.SetValue(entity, items.Count == 0 ? 1 : items.Max(getId) + 1);
                }
                items.Add(entity);
            })
            .Returns(Task.CompletedTask);
        return repo;
    }
}
