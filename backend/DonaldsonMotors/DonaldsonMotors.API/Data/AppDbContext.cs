using DonaldsonMotors.API.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DonaldsonMotors.API.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

        // Standard Entities
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;
        public DbSet<Job> Jobs { get; set; } = null!;
        public DbSet<JobPart> JobParts { get; set; } = null!;
        public DbSet<Part> Parts { get; set; } = null!;
        public DbSet<Supplier> Suppliers { get; set; } = null!;
        public DbSet<Invoice> Invoices { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<ServiceType> ServiceTypes { get; set; } = null!;
        public DbSet<WorkingHours> WorkingHours { get; set; } = null!;
        public DbSet<ScheduleSettings> ScheduleSettings { get; set; } = null!;
        public DbSet<ScheduleException> ScheduleExceptions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // --- NECESSARY CONFIGURATIONS ---

            builder.Entity<ApplicationUser>()
                .HasDiscriminator<string>("UserType")
                .HasValue<Customer>(Entities.Roles.Customer) 
                .HasValue<Employee>(Entities.Roles.Employee);

            // Set composite primary key for the JobPart linking table
            builder.Entity<JobPart>()
                .HasKey(jp => new { jp.JobId, jp.PartId });

            // Set primary key for Vehicle, as it's not the default 'Id'
            builder.Entity<Vehicle>()
                .HasKey(v => v.RegistrationNumber);

            // Set primary key for WorkingHours, as it's not the default 'Id'
            builder.Entity<WorkingHours>()
                .HasKey(wh => wh.DayOfWeek);

            // --- RELATIONSHIP CONFIGURATIONS ---

            
            builder.Entity<Booking>()
                .HasOne(b => b.Mechanic) 
                .WithMany() 
                .HasForeignKey(b => b.MechanicId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull); 

          

            // --- DATA TYPE AND PRECISION CONFIGURATIONS ---

            builder.Entity<Part>().Property(p => p.CostPrice).HasColumnType("decimal(18,2)");
            builder.Entity<Invoice>().Property(i => i.TotalCost).HasColumnType("decimal(18,2)");
            builder.Entity<Job>().Property(j => j.LabourCost).HasColumnType("decimal(18,2)");
            builder.Entity<Job>().Property(j => j.PartsCost).HasColumnType("decimal(18,2)");
            builder.Entity<Part>().Property(p => p.Price).HasColumnType("decimal(18,2)");
            builder.Entity<JobPart>().Property(jp => jp.UnitPrice).HasColumnType("decimal(18,2)");
            builder.Entity<Payment>().Property(p => p.Amount).HasColumnType("decimal(18,2)");
            builder.Entity<ServiceType>().Property(st => st.Price).HasColumnType("decimal(18,2)");

            // --- SEEDING INITIAL DATA ---

            builder.Entity<ScheduleSettings>().HasData(
                new ScheduleSettings
                {
                    Id = 1, 
                    LunchStartTime = new TimeOnly(13, 0),
                    LunchEndTime = new TimeOnly(14, 0)
                }
            );
        }
    }
}