using System.Net;
using System.Net.Http.Json;
using DonaldsonMotors.API.DTOs.Booking;
using DonaldsonMotors.API.DTOs.Part;
using DonaldsonMotors.API.DTOs.Schedule;
using Microsoft.AspNetCore.Mvc;

namespace DonaldsonMotors.Tests.Integration;

/// <summary>A booking's whole life over HTTP, with each role doing its part.</summary>
[Collection(ApiCollection.Name)]
public class BookingFlowTests(ApiFactory app)
{
    private static async Task<DateTime> FirstFreeSlotAsync(HttpClient client)
    {
        var from = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");
        var to = DateTime.UtcNow.AddDays(21).ToString("yyyy-MM-dd");
        var slots = await client.GetFromJsonAsync<List<DateTime>>($"/api/schedule/availability?startDate={from}&endDate={to}");
        return slots![0];
    }

    private static async Task<BookingResponseDto> BookAsync(HttpClient customer, string reg = "SG21ABC", int serviceTypeId = 1)
    {
        var slot = await FirstFreeSlotAsync(customer);
        var response = await customer.PostAsJsonAsync("/api/bookings", new { vehicleRegistrationNumber = reg, serviceTypeId, slotStart = slot });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<BookingResponseDto>())!;
    }

    private static async Task<BookingResponseDto> GetBookingAsync(HttpClient client, int id) =>
        (await client.GetFromJsonAsync<List<BookingResponseDto>>("/api/bookings/search"))!.Single(b => b.Id == id);

    [Fact]
    public async Task ABookingGoesFromPendingToPaid()
    {
        var customer = await app.ClientForAsync(DemoAccounts.Customer);
        var manager = await app.ClientForAsync(DemoAccounts.Manager);
        var mechanic = await app.ClientForAsync(DemoAccounts.Mechanic);
        var accounts = await app.ClientForAsync(DemoAccounts.AccountsClerk);

        // Customer books
        var booking = await BookAsync(customer);
        Assert.Equal("Pending", booking.Status);

        // Manager picks a free mechanic and assigns them
        var mechanics = await manager.GetFromJsonAsync<List<MechanicAvailabilityDto>>(
            $"/api/schedule/available-mechanics?slotStart={booking.SlotStart:O}&serviceTypeId={booking.ServiceTypeId}");
        var sam = mechanics!.Single(m => m.MechanicName == "Sam Mechanic");
        Assert.True(sam.IsAvailable);
        var assigned = await manager.PatchAsJsonAsync("/api/bookings/assign-mechanic", new { bookingId = booking.Id, mechanicId = sam.MechanicId });
        Assert.Equal(HttpStatusCode.NoContent, assigned.StatusCode);
        Assert.Equal("Sam Mechanic", (await GetBookingAsync(customer, booking.Id)).MechanicName);

        // Mechanic does the job and uses two oil filters
        var parts = await mechanic.GetFromJsonAsync<List<PartResponseDto>>("/api/parts");
        var filter = parts!.Single(p => p.Name == "Oil Filter Bosch H1");
        Assert.Equal(HttpStatusCode.NoContent, (await mechanic.PostAsync($"/api/bookings/{booking.Id}/start-job", null)).StatusCode);
        var finished = await mechanic.PostAsJsonAsync($"/api/bookings/{booking.Id}/finish-job", new
        {
            description = "Changed oil and filter",
            labourCost = 50m,
            usedParts = new[] { new { partId = filter.Id, quantity = 2 } },
        });
        Assert.Equal(HttpStatusCode.NoContent, finished.StatusCode);
        var filterAfter = (await mechanic.GetFromJsonAsync<List<PartResponseDto>>("/api/parts"))!.Single(p => p.Id == filter.Id);
        Assert.Equal(filter.CurrentStockLevel - 2, filterAfter.CurrentStockLevel);
        Assert.Equal("AwaitingPayment", (await GetBookingAsync(customer, booking.Id)).Status);

        // Accounts clerk takes the payment
        var paid = await accounts.PatchAsJsonAsync($"/api/bookings/{booking.Id}/mark-as-paid", new { paymentNotes = "Card" });
        Assert.Equal(HttpStatusCode.NoContent, paid.StatusCode);
        Assert.Equal("Paid", (await GetBookingAsync(customer, booking.Id)).Status);
    }

    [Fact]
    public async Task ABookedSlotIsNoLongerOffered()
    {
        var customer = await app.ClientForAsync(DemoAccounts.Customer);

        var booking = await BookAsync(customer);

        Assert.NotEqual(booking.SlotStart, await FirstFreeSlotAsync(customer));
        var again = await customer.PostAsJsonAsync("/api/bookings", new { vehicleRegistrationNumber = "SK18XYZ", serviceTypeId = 1, slotStart = booking.SlotStart });
        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
        var problem = await again.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(409, problem!.Status);
        Assert.Contains("just been booked", problem.Detail);
    }

    [Fact]
    public async Task ACustomerCannotBookAnotherCustomersCar()
    {
        var customer = await app.ClientForAsync(DemoAccounts.Customer);
        var slot = await FirstFreeSlotAsync(customer);

        var response = await customer.PostAsJsonAsync("/api/bookings", new { vehicleRegistrationNumber = "SA70PRS", serviceTypeId = 1, slotStart = slot });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CustomersOnlySeeAndCancelTheirOwnBookings()
    {
        var customer = await app.ClientForAsync(DemoAccounts.Customer);
        var other = await app.ClientForAsync(DemoAccounts.OtherCustomer);
        var booking = await BookAsync(customer);
        var reason = new { reason = "I need to rebook for next week" };

        Assert.DoesNotContain(await other.GetFromJsonAsync<List<BookingResponseDto>>("/api/bookings/search") ?? [], b => b.Id == booking.Id);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.GetAsync($"/api/bookings/search?customerId={booking.CustomerId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.PatchAsJsonAsync($"/api/bookings/{booking.Id}/cancel-by-customer", reason)).StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await customer.PatchAsJsonAsync($"/api/bookings/{booking.Id}/cancel-by-customer", reason)).StatusCode);
        Assert.Equal("Cancelled", (await GetBookingAsync(customer, booking.Id)).Status);
    }

    [Fact]
    public async Task CancellingNeedsAProperReason()
    {
        var customer = await app.ClientForAsync(DemoAccounts.Customer);
        var booking = await BookAsync(customer);

        var response = await customer.PatchAsJsonAsync($"/api/bookings/{booking.Id}/cancel-by-customer", new { reason = "no" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task OnlyTheAssignedMechanicCanStartTheJob()
    {
        var customer = await app.ClientForAsync(DemoAccounts.Customer);
        var manager = await app.ClientForAsync(DemoAccounts.Manager);
        var otherMechanic = await app.ClientForAsync(DemoAccounts.OtherMechanic);
        var booking = await BookAsync(customer);
        var mechanics = await manager.GetFromJsonAsync<List<MechanicAvailabilityDto>>(
            $"/api/schedule/available-mechanics?slotStart={booking.SlotStart:O}&serviceTypeId={booking.ServiceTypeId}");
        var sam = mechanics!.Single(m => m.MechanicName == "Sam Mechanic");
        await manager.PatchAsJsonAsync("/api/bookings/assign-mechanic", new { bookingId = booking.Id, mechanicId = sam.MechanicId });

        var response = await otherMechanic.PostAsync($"/api/bookings/{booking.Id}/start-job", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Assigned", (await GetBookingAsync(customer, booking.Id)).Status);
    }

    [Fact]
    public async Task FinishingAJobWithMorePartsThanInStock_IsAConflict()
    {
        var customer = await app.ClientForAsync(DemoAccounts.Customer);
        var manager = await app.ClientForAsync(DemoAccounts.Manager);
        var mechanic = await app.ClientForAsync(DemoAccounts.Mechanic);
        var booking = await BookAsync(customer);
        var mechanics = await manager.GetFromJsonAsync<List<MechanicAvailabilityDto>>(
            $"/api/schedule/available-mechanics?slotStart={booking.SlotStart:O}&serviceTypeId={booking.ServiceTypeId}");
        var sam = mechanics!.Single(m => m.MechanicName == "Sam Mechanic");
        await manager.PatchAsJsonAsync("/api/bookings/assign-mechanic", new { bookingId = booking.Id, mechanicId = sam.MechanicId });
        await mechanic.PostAsync($"/api/bookings/{booking.Id}/start-job", null);
        var part = (await mechanic.GetFromJsonAsync<List<PartResponseDto>>("/api/parts"))![0];

        var response = await mechanic.PostAsJsonAsync($"/api/bookings/{booking.Id}/finish-job", new
        {
            description = "Tried to fit more than we have",
            labourCost = 10m,
            usedParts = new[] { new { partId = part.Id, quantity = part.CurrentStockLevel + 1 } },
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("InProgress", (await GetBookingAsync(customer, booking.Id)).Status);
    }

    [Fact]
    public async Task OnlyStaffWithTheRightRole_CanTakePayments()
    {
        var mechanic = await app.ClientForAsync(DemoAccounts.Mechanic);

        var response = await mechanic.PatchAsJsonAsync("/api/bookings/1/mark-as-paid", new { paymentNotes = "Cash" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ManagersCanFilterBookings()
    {
        var manager = await app.ClientForAsync(DemoAccounts.Manager);

        var paid = await manager.GetFromJsonAsync<List<BookingResponseDto>>("/api/bookings/search?status=Paid");
        var golf = await manager.GetFromJsonAsync<List<BookingResponseDto>>("/api/bookings/search?vehicleRegistrationNumber=SN16TWK");
        var from = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");
        var upcoming = await manager.GetFromJsonAsync<List<BookingResponseDto>>($"/api/bookings/search?dateFrom={from}");

        Assert.NotEmpty(paid!);
        Assert.All(paid!, b => Assert.Equal("Paid", b.Status));
        Assert.NotEmpty(golf!);
        Assert.All(golf!, b => Assert.Equal("SN16TWK", b.VehicleRegistrationNumber));
        Assert.All(upcoming!, b => Assert.True(b.SlotStart.Date >= DateTime.UtcNow.Date.AddDays(1)));
        Assert.Equal(HttpStatusCode.BadRequest, (await manager.GetAsync("/api/bookings/search?status=Lost")).StatusCode);
    }

    [Fact]
    public async Task TheDashboardAndMyBookingsShowTheRightBookings()
    {
        var manager = await app.ClientForAsync(DemoAccounts.Manager);
        var customer = await app.ClientForAsync(DemoAccounts.Customer);

        var active = await manager.GetFromJsonAsync<List<BookingResponseDto>>("/api/bookings/dashboard");
        var mine = await customer.GetFromJsonAsync<List<BookingResponseDto>>("/api/bookings/my-bookings");

        Assert.All(active!, b => Assert.Contains(b.Status, new[] { "Pending", "Assigned", "InProgress", "AwaitingPayment" }));
        Assert.All(active!, b => Assert.NotEqual("N/A", b.ServiceTypeName));
        Assert.All(mine!, b => Assert.Equal("Jamie Customer", b.CustomerFullName));
        Assert.Equal(HttpStatusCode.Forbidden, (await customer.GetAsync("/api/bookings/dashboard")).StatusCode);
    }

    [Fact]
    public async Task AManagerCanCancelABookingWithAReason()
    {
        var customer = await app.ClientForAsync(DemoAccounts.Customer);
        var manager = await app.ClientForAsync(DemoAccounts.Manager);
        var booking = await BookAsync(customer);

        var response = await manager.PatchAsJsonAsync($"/api/bookings/{booking.Id}/admin-cancel", new { reason = "Garage closed for a burst pipe" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("Cancelled", (await GetBookingAsync(customer, booking.Id)).Status);
        var again = await manager.PatchAsJsonAsync($"/api/bookings/{booking.Id}/admin-cancel", new { reason = "Garage closed for a burst pipe" });
        Assert.Equal(HttpStatusCode.BadRequest, again.StatusCode);
    }
}
