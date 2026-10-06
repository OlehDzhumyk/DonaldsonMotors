using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace DonaldsonMotors.Tests.Unit;

public class ScheduleServiceTests
{
    private readonly FakeUnitOfWork _db = new();
    private readonly ScheduleService _service;

    // Mondays far enough ahead to never be in the past: one in summer time (BST, UTC+1), one in winter (GMT)
    private static readonly DateTime SummerMonday = new(2030, 7, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime WinterMonday = new(2030, 1, 7, 0, 0, 0, DateTimeKind.Utc);

    public ScheduleServiceTests()
    {
        foreach (var day in new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday })
        {
            _db.WorkingHours.Add(new WorkingHours { DayOfWeek = day, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(17, 0) });
        }
        var userManager = new Mock<UserManager<ApplicationUser>>(Mock.Of<IUserStore<ApplicationUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);
        _service = new ScheduleService(_db.Object, userManager.Object, NullLogger<ScheduleService>.Instance);
    }

    private static DateTime Utc(DateTime day, int hour) => DateTime.SpecifyKind(day.AddHours(hour), DateTimeKind.Utc);

    [Fact]
    public async Task Availability_GivesTwoHourSlotsAroundTheLunchBreak_InUkTime()
    {
        var slots = (await _service.GetAvailabilityAsync(WinterMonday, WinterMonday)).ToList();

        // 09–11, 11–13 and 15–17; 13–15 overlaps lunch (13:00–14:00). In winter UK time equals UTC.
        Assert.Equal([Utc(WinterMonday, 9), Utc(WinterMonday, 11), Utc(WinterMonday, 15)], slots);
    }

    [Fact]
    public async Task Availability_StaysAtNineLocalTimeDuringBritishSummerTime()
    {
        var slots = (await _service.GetAvailabilityAsync(SummerMonday, SummerMonday)).ToList();

        // 09:00 BST is 08:00 UTC
        Assert.Equal([Utc(SummerMonday, 8), Utc(SummerMonday, 10), Utc(SummerMonday, 14)], slots);
    }

    [Fact]
    public async Task Availability_SkipsDaysWithoutWorkingHoursAndHolidays()
    {
        _db.ScheduleExceptions.Add(new ScheduleException { Id = 1, Date = DateOnly.FromDateTime(WinterMonday.AddDays(1)), Description = "Holiday" });

        // Monday to Sunday: Tuesday is a holiday, the weekend has no hours
        var slots = await _service.GetAvailabilityAsync(WinterMonday, WinterMonday.AddDays(6));

        var days = slots.Select(s => s.DayOfWeek).Distinct();
        Assert.Equal([DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday], days);
    }

    [Fact]
    public async Task Availability_LeavesOutSlotsThatAreAlreadyBooked()
    {
        _db.Bookings.Add(new Booking { Id = 1, SlotStart = Utc(WinterMonday, 11), Status = BookingStatus.Assigned });
        _db.Bookings.Add(new Booking { Id = 2, SlotStart = Utc(WinterMonday, 15), Status = BookingStatus.Cancelled });

        var slots = await _service.GetAvailabilityAsync(WinterMonday, WinterMonday);

        Assert.Equal([Utc(WinterMonday, 9), Utc(WinterMonday, 15)], slots);
    }

    [Fact]
    public async Task Availability_FollowsAChangedLunchBreak()
    {
        _db.Settings.LunchStartTime = new TimeOnly(12, 0);
        _db.Settings.LunchEndTime = new TimeOnly(12, 30);

        var slots = await _service.GetAvailabilityAsync(WinterMonday, WinterMonday);

        Assert.Equal([Utc(WinterMonday, 9), Utc(WinterMonday, 13), Utc(WinterMonday, 15)], slots);
    }

    [Fact]
    public async Task UpdateWorkingHours_RejectsAClosingTimeBeforeTheOpeningTime()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateWorkingHoursAsync(
            [new() { DayOfWeek = DayOfWeek.Monday, StartTime = "17:00", EndTime = "09:00" }]));
    }

    [Fact]
    public async Task UpdateLunchBreak_RejectsAnInvalidTime()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateLunchBreakAsync(new() { StartTime = "noon", EndTime = "13:00" }));
    }
}
