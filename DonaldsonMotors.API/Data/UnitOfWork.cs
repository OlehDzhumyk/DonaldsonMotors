using DonaldsonMotors.API.Interfaces;
using DonaldsonMotors.API.Repositories;

namespace DonaldsonMotors.API.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IBookingRepository Bookings { get; private set; }
        public ICustomerRepository Customers { get; private set; }
        public IEmployeeRepository Employees { get; private set; }
        public IInvoiceRepository Invoices { get; private set; }
        public IJobRepository Jobs { get; private set; }
        public IPartRepository Parts { get; private set; }
        public IPaymentRepository Payments { get; private set; }
        public ISupplierRepository Suppliers { get; private set; }
        public IUserRepository Users { get; private set; }
        public IVehicleRepository Vehicles { get; private set; }
        public IScheduleRepository Schedule { get; private set; }
        public IServiceTypeRepository ServiceTypes { get; private set; }
        public IJobPartRepository JobParts { get; private set; }


        public UnitOfWork(AppDbContext context)
        {
            _context = context;

            Bookings = new BookingRepository(_context);
            Customers = new CustomerRepository(_context);
            Employees = new EmployeeRepository(_context);
            Invoices = new InvoiceRepository(_context);
            Jobs = new JobRepository(_context);
            Parts = new PartRepository(_context);
            Payments = new PaymentRepository(_context);
            Suppliers = new SupplierRepository(_context);
            Users = new UserRepository(_context);
            Vehicles = new VehicleRepository(_context);
            Schedule = new ScheduleRepository(_context);
            ServiceTypes = new ServiceTypeRepository(_context);
            JobParts = new JobPartRepository(_context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}