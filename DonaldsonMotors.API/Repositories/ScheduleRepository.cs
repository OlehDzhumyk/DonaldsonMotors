using DonaldsonMotors.API.Data;
using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DonaldsonMotors.API.Repositories
{
    
    /// <summary>
    /// Handles data access for multiple schedule-related entities.
    /// </summary>
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly AppDbContext _context;
        public ScheduleRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<WorkingHours>> GetWorkingHoursAsync() =>
            await _context.WorkingHours.ToListAsync();

        public async Task UpdateWorkingHoursAsync(IEnumerable<WorkingHours> workingHours)
        {
            var allHours = await _context.WorkingHours.ToListAsync();
            _context.WorkingHours.RemoveRange(allHours);
            await _context.WorkingHours.AddRangeAsync(workingHours);
        }

        public async Task<ScheduleSettings?> GetSettingsAsync() =>
            await _context.ScheduleSettings.FirstOrDefaultAsync(s => s.Id == 1);

        public void UpdateSettings(ScheduleSettings settings) =>
            _context.ScheduleSettings.Update(settings);

        public async Task<IEnumerable<ScheduleException>> GetExceptionsAsync() =>
            await _context.ScheduleExceptions.OrderBy(e => e.Date).ToListAsync();

        public async Task<ScheduleException?> GetExceptionByIdAsync(int id) =>
            await _context.ScheduleExceptions.FindAsync(id);

        public async Task AddExceptionAsync(ScheduleException exception) =>
            await _context.ScheduleExceptions.AddAsync(exception);

        public void DeleteException(ScheduleException exception) =>
            _context.ScheduleExceptions.Remove(exception);
    }
}