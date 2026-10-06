using DonaldsonMotors.API.Data;
using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DonaldsonMotors.Tests.Integration;

/// <summary>
/// The database itself guards against two requests racing each other,
/// so the checks in the services are not the only line of defence.
/// </summary>
[Collection(ApiCollection.Name)]
public class ConcurrencyTests(ApiFactory app)
{
    private AppDbContext NewContext() =>
        app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();

    private static Booking CopyOf(Booking booking, BookingStatus status) => new()
    {
        CustomerId = booking.CustomerId,
        VehicleRegistrationNumber = booking.VehicleRegistrationNumber,
        ServiceTypeId = booking.ServiceTypeId,
        SlotStart = booking.SlotStart,
        Status = status,
    };

    [Fact]
    public async Task TheDatabaseRefusesASecondLiveBookingForTheSameSlot()
    {
        await using var db = NewContext();
        var taken = await db.Bookings.AsNoTracking().FirstAsync(b => b.Status != BookingStatus.Cancelled);

        db.Bookings.Add(CopyOf(taken, BookingStatus.Pending));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task ACancelledBookingDoesNotHoldItsSlot()
    {
        await using var db = NewContext();
        var taken = await db.Bookings.AsNoTracking().FirstAsync(b => b.Status != BookingStatus.Cancelled);

        db.Bookings.Add(CopyOf(taken, BookingStatus.Cancelled));

        Assert.Equal(1, await db.SaveChangesAsync());
    }

    [Fact]
    public async Task AStockChangeBasedOnAStaleReadIsRejected()
    {
        await using var first = NewContext();
        await using var second = NewContext();
        var partId = (await first.Parts.FirstAsync(p => p.CurrentStockLevel > 1)).Id;
        var mine = await first.Parts.SingleAsync(p => p.Id == partId);
        var theirs = await second.Parts.SingleAsync(p => p.Id == partId);

        theirs.CurrentStockLevel -= 1;
        await second.SaveChangesAsync();
        mine.CurrentStockLevel -= 1;

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => new UnitOfWork(first).CompleteAsync());
    }
}
