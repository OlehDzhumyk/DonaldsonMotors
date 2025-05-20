// Data/AppDbContext.cs
using DonaldsonMotors.API.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DonaldsonMotors.API.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Only declare DbSet for aggregate roots / tables you need to query directly
        public DbSet<VehicleEntity> Vehicles { get; set; }
        public DbSet<BookingEntity> Bookings { get; set; }
        public DbSet<JobEntity> Jobs { get; set; }
        public DbSet<JobItemEntity> JobItems { get; set; }
        public DbSet<ItemEntity> Items { get; set; }
        public DbSet<SupplierEntity> Suppliers { get; set; }
        public DbSet<InvoiceEntity> Invoices { get; set; }
        public DbSet<PaymentEntity> Payments { get; set; }
        public DbSet<ServiceType> ServiceTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // --- TPH for ApplicationUser ---
            builder.Entity<ApplicationUser>()
                .ToTable("AspNetUsers")  // keep identity table name
                .HasDiscriminator<string>("UserType")
                .HasValue<CustomerEntity>("Customer")
                .HasValue<EmployeeEntity>("Employee");

            // --- Identity roles table name ---
            builder.Entity<ApplicationRole>()
                .ToTable("AspNetRoles");

            // --- VehicleEntity ---
            builder.Entity<VehicleEntity>(entity =>
            {
                entity.HasKey(v => v.RegistrationNumber);
                entity.Property(v => v.Make).IsRequired();
                entity.Property(v => v.Model).IsRequired();
                entity.HasOne(v => v.Customer)
                      .WithMany(c => c.Vehicles)
                      .HasForeignKey(v => v.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // --- BookingEntity ---
            builder.Entity<BookingEntity>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.VehicleRegistration).IsRequired();
                entity.Property(b => b.Status).IsRequired();

                entity.HasOne(b => b.Customer)
                      .WithMany()      // do not rely on DbSet<CustomerEntity>
                      .HasForeignKey(b => b.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.Vehicle)
                      .WithMany()
                      .HasForeignKey(b => b.VehicleRegistration)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.Invoice)
                      .WithOne(i => i.Booking)
                      .HasForeignKey<InvoiceEntity>(i => i.BookingId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // --- InvoiceEntity ---
            builder.Entity<InvoiceEntity>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.TotalCost).HasColumnType("decimal(18,2)");
                entity.HasOne(i => i.Booking)
                      .WithOne(b => b.Invoice)
                      .HasForeignKey<InvoiceEntity>(i => i.BookingId);
            });

            // --- PaymentEntity ---
            builder.Entity<PaymentEntity>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Amount).HasColumnType("decimal(18,2)");
                entity.HasOne(p => p.Invoice)
                      .WithMany(i => i.Payments)
                      .HasForeignKey(p => p.InvoiceId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // --- JobEntity ---
            builder.Entity<JobEntity>(entity =>
            {
                entity.HasKey(j => j.Id);
                entity.Property(j => j.Description).IsRequired();
                entity.HasOne(j => j.Booking)
                      .WithMany(b => b.Jobs)
                      .HasForeignKey(j => j.BookingId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(j => j.Technician)
                      .WithMany(e => e.JobsCompleted)
                      .HasForeignKey(j => j.TechnicianId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // --- JobItemEntity (many-to-many) ---
            builder.Entity<JobItemEntity>(entity =>
            {
                entity.HasKey(ji => new { ji.JobEntityId, ji.ItemEntityId });
                entity.HasOne(ji => ji.Job)
                      .WithMany(j => j.Items)
                      .HasForeignKey(ji => ji.JobEntityId);
                entity.HasOne(ji => ji.Item)
                      .WithMany(i => i.JobItems)
                      .HasForeignKey(ji => ji.ItemEntityId);
            });

            // --- ItemEntity ---
            builder.Entity<ItemEntity>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Price).HasColumnType("decimal(18,2)");
                entity.HasOne(i => i.Supplier)
                      .WithMany(s => s.Items)
                      .HasForeignKey(i => i.SupplierId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // --- SupplierEntity ---
            builder.Entity<SupplierEntity>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Name).IsRequired();
            });

            // --- ServiceType ---
            builder.Entity<ServiceType>(entity =>
            {
                entity.HasKey(st => st.Id);
                entity.Property(st => st.Name).IsRequired();
                entity.Property(st => st.Price).HasColumnType("decimal(18,2)");
            });
        }
    }
}
