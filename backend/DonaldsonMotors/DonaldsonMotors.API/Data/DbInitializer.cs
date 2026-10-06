using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DonaldsonMotors.API.Data
{
    public class DbInitializer 
    {
        public static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                // Resolving services from the scope is correct here
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
                // This line correctly gets a logger instance for the DbInitializer context
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<DbInitializer>>();

                logger.LogInformation("Attempting to seed initial data...");

                // 1. Seed Roles
                string[] roleNames = { Roles.Manager, Roles.Mechanic, Roles.StockController, Roles.AccountsClerk, Roles.Customer };
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new ApplicationRole { Name = roleName, NormalizedName = roleName.ToUpper() });
                        logger.LogInformation("Role '{RoleName}' created.", roleName);
                    }
                }

                // 2. Seed Default Manager User
                var managerEmail = "manager@donaldson.com";
                if (await userManager.FindByEmailAsync(managerEmail) == null)
                {
                    var managerUser = new Employee
                    {
                        UserName = managerEmail,
                        Email = managerEmail,
                        FullName = "Default Manager",
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(managerUser, "Password123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(managerUser, Roles.Manager);
                        logger.LogInformation("Default manager '{ManagerEmail}' created and assigned to Manager role.", managerEmail);
                    }
                    else
                    {
                        logger.LogError("Failed to create default manager: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }

                // Without working hours the availability endpoint returns no slots, so start with Mon–Fri 09:00–17:00.
                // The manager can change them later through the Schedule endpoints.
                if (!await context.WorkingHours.AnyAsync())
                {
                    foreach (var day in new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday })
                    {
                        context.WorkingHours.Add(new WorkingHours { DayOfWeek = day, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(17, 0) });
                    }
                    await context.SaveChangesAsync();
                    logger.LogInformation("Seeded default working hours.");
                }

                // 3. Seed Service Types
                if (!await context.ServiceTypes.AnyAsync())
                {
                    context.ServiceTypes.AddRange(
                        new ServiceType { Name = "Oil Change", Description = "Standard oil and filter change.", Price = 50.00m, DurationHours = 1.0 },
                        new ServiceType { Name = "Tyre Replacement (x1)", Description = "Replacement of one tyre.", Price = 20.00m, DurationHours = 0.5 },
                        new ServiceType { Name = "Annual Service", Description = "Full annual vehicle check-up.", Price = 150.00m, DurationHours = 3.0 },
                        new ServiceType { Name = "Brake Pad Replacement", Description = "Front or rear brake pad replacement.", Price = 80.00m, DurationHours = 2.0 }
                    );
                    // Save changes for ServiceTypes before needing their potential IDs, though not strictly needed here.
                    // await context.SaveChangesAsync(); 
                    logger.LogInformation("Seeded ServiceTypes.");
                }

                // 4. Seed Suppliers
                Supplier? supplierEuroParts = await context.Suppliers.FirstOrDefaultAsync(s => s.Name == "Euro Car Parts");
                Supplier? supplierMotorFactors = await context.Suppliers.FirstOrDefaultAsync(s => s.Name == "General Motor Factors");

                if (supplierEuroParts == null)
                {
                    supplierEuroParts = new Supplier { Name = "Euro Car Parts", AddressLine1 = "123 Auto Rd", Postcode = "G1 1AA", Email = "sales@europarts.com" };
                    context.Suppliers.Add(supplierEuroParts);
                }
                if (supplierMotorFactors == null)
                {
                    supplierMotorFactors = new Supplier { Name = "General Motor Factors", AddressLine1 = "456 Spares Ave", Postcode = "G2 2BB", Email = "orders@motofactors.com" };
                    context.Suppliers.Add(supplierMotorFactors);
                }
                // Save if any new suppliers were added, to ensure IDs are generated before seeding parts
                if (context.ChangeTracker.HasChanges())
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation("Seeded or ensured Suppliers exist.");
                }

                // 5. Seed Parts
                if (!await context.Parts.AnyAsync())
                {
                    // Ensure IDs are available if they were just created
                    var euroPartsId = (await context.Suppliers.FirstAsync(s => s.Name == "Euro Car Parts")).Id;
                    var motorFactorsId = (await context.Suppliers.FirstAsync(s => s.Name == "General Motor Factors")).Id;

                    context.Parts.AddRange(
                        new Part { Name = "Oil Filter Bosch H1", Price = 8.50m, CostPrice = 5.20m, CurrentStockLevel = 50, SupplierId = euroPartsId },
                        new Part { Name = "5W-30 Synthetic Oil (5L)", Price = 25.00m, CostPrice = 17.50m, CurrentStockLevel = 30, SupplierId = euroPartsId },
                        new Part { Name = "Brake Pads - Front Set Brembo", Price = 45.00m, CostPrice = 31.00m, CurrentStockLevel = 20, SupplierId = motorFactorsId },
                        new Part { Name = "Spark Plug NGK BKR6E-11", Price = 4.00m, CostPrice = 2.40m, CurrentStockLevel = 100, SupplierId = motorFactorsId },
                        new Part { Name = "All Season Tyre 205/55 R16", Price = 65.00m, CostPrice = 46.00m, CurrentStockLevel = 40, SupplierId = euroPartsId }
                    );
                    logger.LogInformation("Seeded Parts.");
                }

                // Final save for any pending changes (like Parts if seeded)
                if (context.ChangeTracker.HasChanges())
                {
                    await context.SaveChangesAsync();
                }

                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                if (configuration.GetValue<bool>("SeedDemoData"))
                {
                    await SeedDemoDataAsync(context, userManager, logger);
                }
                logger.LogInformation("Data seeding completed.");
            }
        }
    

        /// <summary>
        /// One account per role, a few customers with cars, and bookings in every state
        /// (with jobs, parts, invoices and payments) so every page has something to show.
        /// All demo accounts use the password "Password123!".
        /// </summary>
        private static async Task SeedDemoDataAsync(AppDbContext context, UserManager<ApplicationUser> userManager, ILogger logger)
        {
            if (await userManager.FindByEmailAsync("customer@donaldson.com") != null)
            {
                return;
            }

            const string password = "Password123!";
            var sam = (Employee)await CreateUserAsync(userManager, new Employee { FullName = "Sam Mechanic" }, "mechanic@donaldson.com", password, Roles.Mechanic);
            var lee = (Employee)await CreateUserAsync(userManager, new Employee { FullName = "Lee Fraser" }, "lee@donaldson.com", password, Roles.Mechanic);
            await CreateUserAsync(userManager, new Employee { FullName = "Alex Stock" }, "stock@donaldson.com", password, Roles.StockController);
            await CreateUserAsync(userManager, new Employee { FullName = "Jo Accounts" }, "accounts@donaldson.com", password, Roles.AccountsClerk);
            var jamie = (Customer)await CreateUserAsync(userManager, new Customer { FullName = "Jamie Customer", Address = "1 Demo Street, Glasgow", PhoneNumber = "07700 900123" }, "customer@donaldson.com", password, Roles.Customer);
            var priya = (Customer)await CreateUserAsync(userManager, new Customer { FullName = "Priya Shah", PhoneNumber = "07700 900456" }, "priya@example.com", password, Roles.Customer);
            var tom = (Customer)await CreateUserAsync(userManager, new Customer { FullName = "Tom Walker" }, "tom@example.com", password, Roles.Customer);

            context.Vehicles.AddRange(
                new Vehicle { RegistrationNumber = "SG21ABC", Make = "Ford", Model = "Focus", Year = 2021, Mileage = 24000, OwnerId = jamie.Id },
                new Vehicle { RegistrationNumber = "SK18XYZ", Make = "Vauxhall", Model = "Corsa", Year = 2018, Mileage = 61000, OwnerId = jamie.Id },
                new Vehicle { RegistrationNumber = "SA70PRS", Make = "Toyota", Model = "Yaris", Year = 2020, Mileage = 33500, OwnerId = priya.Id },
                new Vehicle { RegistrationNumber = "SN16TWK", Make = "Volkswagen", Model = "Golf", Year = 2016, Mileage = 88200, OwnerId = tom.Id });

            var services = await context.ServiceTypes.ToDictionaryAsync(st => st.Name);
            var parts = await context.Parts.ToDictionaryAsync(p => p.Name);
            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
            var next = NextWeekday(today.AddDays(1));
            var later = NextWeekday(next.AddDays(1));

            // Slots are garage wall-clock times (see GarageTime), stored in UTC
            static DateTime At(DateTime day, int hour) => DateTime.SpecifyKind(GarageTime.ToUtc(day, new TimeOnly(hour, 0)), DateTimeKind.Utc);

            Booking Book(Customer customer, string reg, string service, DateTime slot, BookingStatus status, Employee? mechanic = null) =>
                new() { CustomerId = customer.Id, VehicleRegistrationNumber = reg, ServiceTypeId = services[service].Id, SlotStart = slot, Status = status, MechanicId = mechanic?.Id };

            // Upcoming work
            context.Bookings.AddRange(
                Book(jamie, "SG21ABC", "Oil Change", At(next, 9), BookingStatus.Pending),
                Book(priya, "SA70PRS", "Annual Service", At(later, 11), BookingStatus.Pending),
                Book(jamie, "SK18XYZ", "Brake Pad Replacement", At(next, 11), BookingStatus.Assigned, sam));

            // Being worked on today
            var inProgress = Book(tom, "SN16TWK", "Tyre Replacement (x1)", At(PreviousWeekday(today), 9), BookingStatus.InProgress, lee);
            context.Bookings.Add(inProgress);
            context.Jobs.Add(new Job { Booking = inProgress, MechanicId = lee.Id, StartDate = DateTime.UtcNow.AddHours(-1), Description = "Work started on Tyre Replacement (x1)" });

            // Finished jobs: one waiting for payment, two paid
            AddFinishedJob(context, Book(priya, "SA70PRS", "Brake Pad Replacement", At(PreviousWeekday(today.AddDays(-1)), 15), BookingStatus.AwaitingPayment, sam),
                "Replaced front brake pads, checked discs and topped up brake fluid.", 80m, [(parts["Brake Pads - Front Set Brembo"], 1)], paidMethod: null);
            AddFinishedJob(context, Book(tom, "SN16TWK", "Oil Change", At(PreviousWeekday(today.AddDays(-3)), 11), BookingStatus.Paid, lee),
                "Drained oil, replaced oil filter, refilled with 5W-30.", 50m, [(parts["Oil Filter Bosch H1"], 1), (parts["5W-30 Synthetic Oil (5L)"], 1)], paidMethod: "Manual (Card at collection)");
            AddFinishedJob(context, Book(jamie, "SG21ABC", "Annual Service", At(PreviousWeekday(today.AddDays(-10)), 9), BookingStatus.Paid, sam),
                "Full annual service: oil, filters, spark plugs, fluid levels and safety checks.", 150m, [(parts["Spark Plug NGK BKR6E-11"], 4), (parts["Oil Filter Bosch H1"], 1)], paidMethod: "Manual (Cash)");

            var cancelled = Book(jamie, "SK18XYZ", "Oil Change", At(PreviousWeekday(today.AddDays(-5)), 15), BookingStatus.Cancelled);
            cancelled.CancellationReason = "Customer: Car was off the road that week, will rebook.";
            cancelled.CancelledAt = DateTime.UtcNow.AddDays(-7);
            cancelled.CancelledByUserId = jamie.Id;
            context.Bookings.Add(cancelled);

            await context.SaveChangesAsync();
            logger.LogInformation("Seeded demo accounts, vehicles and bookings.");
        }

        private static void AddFinishedJob(AppDbContext context, Booking booking, string description, decimal labour,
            (Part Part, int Quantity)[] usedParts, string? paidMethod)
        {
            var partsCost = usedParts.Sum(p => p.Part.Price * p.Quantity);
            var job = new Job
            {
                Booking = booking,
                MechanicId = booking.MechanicId!.Value,
                Description = description,
                LabourCost = labour,
                PartsCost = partsCost,
                StartDate = booking.SlotStart,
                CompletionDate = booking.SlotStart.AddHours(2),
                JobParts = usedParts.Select(p => new JobPart { PartId = p.Part.Id, QuantityUsed = p.Quantity, UnitPrice = p.Part.Price }).ToList(),
            };
            var invoice = new Invoice { Booking = booking, TotalCost = labour + partsCost, DateIssued = job.CompletionDate.Value };
            context.Bookings.Add(booking);
            context.Jobs.Add(job);
            context.Invoices.Add(invoice);

            if (paidMethod != null)
            {
                booking.PaidAt = invoice.DateIssued.AddDays(1);
                context.Payments.Add(new Payment { Invoice = invoice, Amount = invoice.TotalCost, Method = paidMethod, DatePaid = booking.PaidAt.Value });
            }
        }

        private static async Task<ApplicationUser> CreateUserAsync(UserManager<ApplicationUser> userManager, ApplicationUser user, string email, string password, string role)
        {
            user.Email = email;
            user.UserName = email;
            user.EmailConfirmed = true;
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Could not create demo user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            await userManager.AddToRoleAsync(user, role);
            return user;
        }

        private static DateTime PreviousWeekday(DateTime date)
        {
            while (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                date = date.AddDays(-1);
            }
            return DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }

        private static DateTime NextWeekday(DateTime date)
        {
            while (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                date = date.AddDays(1);
            }
            return DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }
    }
}