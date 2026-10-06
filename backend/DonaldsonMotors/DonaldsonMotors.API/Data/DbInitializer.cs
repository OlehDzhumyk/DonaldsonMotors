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
                logger.LogInformation("Data seeding completed.");
            }
        }
    }
}