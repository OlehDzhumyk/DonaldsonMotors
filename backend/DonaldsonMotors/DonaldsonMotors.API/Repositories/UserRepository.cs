using DonaldsonMotors.API.Data;
using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DonaldsonMotors.API.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context  )
        {
            _context = context;
        }

        // Generic User Methods
        public async Task<ApplicationUser?> GetUserByIdAsync(int id) =>
            await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            var normalizedEmail = email.ToUpperInvariant();
            return await _context.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);
        }

        // Customer-Specific Methods
        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            return await _context.Users.OfType<Customer>()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Customer?> GetCustomerByIdWithVehiclesAsync(int id)
        {
            return await _context.Users.OfType<Customer>()
                .Include(c => c.Vehicles)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        // Employee-Specific Methods
        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            return await _context.Users.OfType<Employee>()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            return await _context.Users.OfType<Employee>().ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetEmployeesInRoleAsync(string roleName)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null) return Enumerable.Empty<Employee>();

            var userIdsInRole = await _context.UserRoles
                .Where(ur => ur.RoleId == role.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            return await _context.Users.OfType<Employee>()
                .Where(e => userIdsInRole.Contains(e.Id))
                .ToListAsync();
           
        }


        /// <summary>
        /// Gets all active (non-cancelled, non-paid, non-archived) bookings for a specific mechanic on a given date,
        /// including their ServiceType for duration calculation.
        /// </summary>
        public async Task<IEnumerable<Booking>> GetMechanicBookingsOnDateAsync(int mechanicId, DateTime date)
        {
            var activeStatuses = new[] {
                BookingStatus.Pending,
                BookingStatus.Assigned,
                BookingStatus.InProgress,
                BookingStatus.AwaitingPayment 
            };

            return await _context.Bookings
                .Include(b => b.ServiceType)
                .Where(b =>
                    b.MechanicId == mechanicId &&
                    activeStatuses.Contains(b.Status) &&
                    b.SlotStart.Date == date.Date) // Compare only the Date part
                .OrderBy(b => b.SlotStart) // Good for iterating in the service
                .ToListAsync();
        }
    }
}