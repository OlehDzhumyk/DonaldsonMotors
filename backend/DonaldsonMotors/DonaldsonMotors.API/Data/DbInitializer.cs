using DonaldsonMotors.API.Data.Entities;
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
                        new Part { Name = "Oil Filter Bosch H1", Price = 8.50m, CurrentStockLevel = 50, SupplierId = euroPartsId },
                        new Part { Name = "5W-30 Synthetic Oil (5L)", Price = 25.00m, CurrentStockLevel = 30, SupplierId = euroPartsId },
                        new Part { Name = "Brake Pads - Front Set Brembo", Price = 45.00m, CurrentStockLevel = 20, SupplierId = motorFactorsId },
                        new Part { Name = "Spark Plug NGK BKR6E-11", Price = 4.00m, CurrentStockLevel = 100, SupplierId = motorFactorsId },
                        new Part { Name = "All Season Tyre 205/55 R16", Price = 65.00m, CurrentStockLevel = 40, SupplierId = euroPartsId }
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
        /// One account per role, a customer with two cars and a few upcoming bookings,
        /// so every page has something to show right after `docker compose up`.
        /// All demo accounts use the password "Password123!".
        /// </summary>
        private static async Task SeedDemoDataAsync(AppDbContext context, UserManager<ApplicationUser> userManager, ILogger logger)
        {
            if (await userManager.FindByEmailAsync("customer@donaldson.com") != null)
            {
                return;
            }

            const string password = "Password123!";
            var mechanic = await CreateUserAsync(userManager, new Employee { FullName = "Sam Mechanic" }, "mechanic@donaldson.com", password, Roles.Mechanic);
            await CreateUserAsync(userManager, new Employee { FullName = "Alex Stock" }, "stock@donaldson.com", password, Roles.StockController);
            await CreateUserAsync(userManager, new Employee { FullName = "Jo Accounts" }, "accounts@donaldson.com", password, Roles.AccountsClerk);
            var customer = (Customer)await CreateUserAsync(userManager, new Customer { FullName = "Jamie Customer", Address = "1 Demo Street, Glasgow" }, "customer@donaldson.com", password, Roles.Customer);

            context.Vehicles.AddRange(
                new Vehicle { RegistrationNumber = "SG21ABC", Make = "Ford", Model = "Focus", Year = 2021, Mileage = 24000, OwnerId = customer.Id },
                new Vehicle { RegistrationNumber = "SK18XYZ", Make = "Vauxhall", Model = "Corsa", Year = 2018, Mileage = 61000, OwnerId = customer.Id });

            var serviceTypes = await context.ServiceTypes.OrderBy(st => st.Id).ToListAsync();
            var day = NextWeekday(DateTime.UtcNow.Date.AddDays(1));
            var nextDay = NextWeekday(day.AddDays(1));
            context.Bookings.AddRange(
                new Booking { CustomerId = customer.Id, VehicleRegistrationNumber = "SG21ABC", ServiceTypeId = serviceTypes[0].Id, SlotStart = day.AddHours(9), Status = BookingStatus.Pending },
                new Booking { CustomerId = customer.Id, VehicleRegistrationNumber = "SK18XYZ", ServiceTypeId = serviceTypes[3].Id, SlotStart = day.AddHours(11), Status = BookingStatus.Assigned, MechanicId = mechanic.Id },
                new Booking { CustomerId = customer.Id, VehicleRegistrationNumber = "SG21ABC", ServiceTypeId = serviceTypes[2].Id, SlotStart = nextDay.AddHours(9), Status = BookingStatus.Pending });

            await context.SaveChangesAsync();
            logger.LogInformation("Seeded demo accounts, vehicles and bookings.");
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