using DonaldsonMotors.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DonaldsonMotors.API.Data
{
    public class AppDbContext
      : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts)
            : base(opts) { }

        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;
        public DbSet<Job> Jobs { get; set; } = null!;
        public DbSet<JobItem> JobItems { get; set; } = null!;
        public DbSet<Item> Items { get; set; } = null!;
        public DbSet<Supplier> Suppliers { get; set; } = null!;
        public DbSet<Invoice> Invoices { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // TPH discriminator on ApplicationUser
            builder.Entity<ApplicationUser>()
                .HasDiscriminator<string>("UserType")
                .HasValue<Customer>(Models.Roles.Customer)
                .HasValue<Employee>(Models.Roles.Employee);

            // Composite PK for JobItem
            builder.Entity<JobItem>()
                   .HasKey(ji => new { ji.JobId, ji.ItemId });

            // Vehicle PK
            builder.Entity<Vehicle>()
                   .HasKey(v => v.RegistrationNumber);

            // Customer → Booking
            builder.Entity<Booking>()
                   .HasOne(b => b.User)
                   .WithMany(c => c.Bookings)
                   .HasForeignKey(b => b.CustomerId);

            // Vehicle → Booking
            builder.Entity<Booking>()
                   .HasOne(b => b.Vehicle)
                   .WithMany(v => v.Bookings)
                   .HasForeignKey(b => b.VehicleRegistration);

            // Booking → Invoice (1:1)
            builder.Entity<Invoice>()
                   .HasOne(inv => inv.Booking)
                   .WithOne(b => b.Invoice)
                   .HasForeignKey<Invoice>(inv => inv.BookingId);

            // Invoice → Payments (1:N)
            builder.Entity<Payment>()
                   .HasOne(p => p.Invoice)
                   .WithMany(inv => inv.Payments)
                   .HasForeignKey(p => p.InvoiceId);

            // Employee → Job
            builder.Entity<Job>()
                   .HasOne(j => j.Technician)
                   .WithMany(e => e.JobsCompleted)
                   .HasForeignKey(j => j.TechnicianId);

            // Booking → Job
            builder.Entity<Job>()
                   .HasOne(j => j.Booking)
                   .WithMany(b => b.Jobs)
                   .HasForeignKey(j => j.BookingId);

            // JobItem → Job & Item
            builder.Entity<JobItem>()
                   .HasOne(ji => ji.Job)
                   .WithMany(j => j.JobItems)
                   .HasForeignKey(ji => ji.JobId);
            builder.Entity<JobItem>()
                   .HasOne(ji => ji.Item)
                   .WithMany(i => i.JobItems)
                   .HasForeignKey(ji => ji.ItemId);

            // Supplier → Item
            builder.Entity<Item>()
                   .HasOne(i => i.Supplier)
                   .WithMany(s => s.Items)
                   .HasForeignKey(i => i.SupplierId);
        }
    }
}
