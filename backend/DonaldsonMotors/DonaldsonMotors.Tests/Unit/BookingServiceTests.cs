using System.Security.Claims;
using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.DTOs.Booking;
using DonaldsonMotors.API.Exceptions;
using DonaldsonMotors.API.Interfaces;
using DonaldsonMotors.API.Services;
using DonaldsonMotors.API.ViewModels.Invoice;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace DonaldsonMotors.Tests.Unit;

public class BookingServiceTests
{
    private const int CustomerId = 10, OtherCustomerId = 11, MechanicId = 20, OtherMechanicId = 21;

    private readonly FakeUnitOfWork _db = new();
    private readonly Mock<IEmailService> _email = new();
    private readonly BookingService _service;
    private readonly DateTime _tomorrow9 = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(1).AddHours(9), DateTimeKind.Utc);

    public BookingServiceTests()
    {
        var customer = new Customer { Id = CustomerId, FullName = "Jamie", Email = "jamie@example.com" };
        customer.Vehicles.Add(new Vehicle { RegistrationNumber = "SG21ABC", Make = "Ford", Model = "Focus", OwnerId = CustomerId });
        var other = new Customer { Id = OtherCustomerId, FullName = "Priya", Email = "priya@example.com" };
        other.Vehicles.Add(new Vehicle { RegistrationNumber = "SA70PRS", Make = "Toyota", Model = "Yaris", OwnerId = OtherCustomerId });
        _db.Customers.AddRange(customer, other);
        _db.Employees.AddRange(new Employee { Id = MechanicId, FullName = "Sam" }, new Employee { Id = OtherMechanicId, FullName = "Lee" });
        _db.ServiceTypes.Add(new ServiceType { Id = 1, Name = "Brake Pad Replacement", Price = 80m, DurationHours = 2 });
        _db.Parts.Add(new Part { Id = 1, Name = "Brake pads", Price = 45m, CurrentStockLevel = 3, SupplierId = 1 });

        _service = new BookingService(_db.Object, _email.Object, NullLogger<BookingService>.Instance);
    }

    private Booking AddBooking(BookingStatus status, int? mechanicId = null, DateTime? slot = null, int customerId = CustomerId)
    {
        var booking = new Booking
        {
            Id = _db.Bookings.Count + 1,
            CustomerId = customerId,
            Customer = _db.Customers.Single(c => c.Id == customerId),
            VehicleRegistrationNumber = "SG21ABC",
            ServiceTypeId = 1,
            ServiceType = _db.ServiceTypes[0],
            SlotStart = slot ?? _tomorrow9,
            Status = status,
            MechanicId = mechanicId,
            Jobs = [],
        };
        _db.Bookings.Add(booking);
        return booking;
    }

    private static CreateBookingRequestDto Request(DateTime slot, string reg = "SG21ABC") =>
        new() { VehicleRegistrationNumber = reg, ServiceTypeId = 1, SlotStart = slot };

    // --- Creating bookings ---

    [Fact]
    public async Task CreateBooking_SavesAPendingBookingAndEmailsTheCustomer()
    {
        var result = await _service.CreateBookingAsync(CustomerId, Request(_tomorrow9));

        var saved = Assert.Single(_db.Bookings);
        Assert.Equal(BookingStatus.Pending, saved.Status);
        Assert.Equal("Pending", result.Status);
        Assert.Equal(1, _db.SaveCount);
        _email.Verify(e => e.SendBookingConfirmedAsync("jamie@example.com", "Jamie", _tomorrow9, saved.Id, "Brake Pad Replacement", "SG21ABC"), Times.Once);
    }

    [Fact]
    public async Task CreateBooking_RejectsASlotInThePast()
    {
        await Assert.ThrowsAsync<SlotUnavailableException>(() => _service.CreateBookingAsync(CustomerId, Request(DateTime.UtcNow.AddHours(-1))));
        Assert.Empty(_db.Bookings);
    }

    [Fact]
    public async Task CreateBooking_RejectsAnotherCustomersVehicle()
    {
        await Assert.ThrowsAsync<VehicleAccessDeniedException>(() => _service.CreateBookingAsync(CustomerId, Request(_tomorrow9, "SA70PRS")));
    }

    [Fact]
    public async Task CreateBooking_RejectsAnUnknownVehicle()
    {
        await Assert.ThrowsAsync<VehicleNotFoundException>(() => _service.CreateBookingAsync(CustomerId, Request(_tomorrow9, "NOPE123")));
    }

    [Fact]
    public async Task CreateBooking_RejectsASlotThatIsAlreadyTaken()
    {
        AddBooking(BookingStatus.Pending, customerId: OtherCustomerId);

        await Assert.ThrowsAsync<SlotUnavailableException>(() => _service.CreateBookingAsync(CustomerId, Request(_tomorrow9)));
    }

    [Fact]
    public async Task CreateBooking_CanReuseTheSlotOfACancelledBooking()
    {
        AddBooking(BookingStatus.Cancelled, customerId: OtherCustomerId);

        await _service.CreateBookingAsync(CustomerId, Request(_tomorrow9));

        Assert.Equal(2, _db.Bookings.Count);
    }

    // --- Assigning mechanics ---

    [Fact]
    public async Task AssignMechanic_MovesAPendingBookingToAssigned()
    {
        var booking = AddBooking(BookingStatus.Pending);

        await _service.AssignMechanicAsync(new AssignMechanicRequestDto { BookingId = booking.Id, MechanicId = MechanicId });

        Assert.Equal(BookingStatus.Assigned, booking.Status);
        Assert.Equal(MechanicId, booking.MechanicId);
        _email.Verify(e => e.SendTechnicianAssignedAsync("jamie@example.com", "Jamie", "Sam", booking.SlotStart, booking.Id), Times.Once);
    }

    [Fact]
    public async Task AssignMechanic_RefusesAMechanicWhoIsBusyAtThatTime()
    {
        // The mechanic already has a two-hour job starting an hour earlier
        AddBooking(BookingStatus.Assigned, MechanicId, _tomorrow9.AddHours(-1), OtherCustomerId);
        var booking = AddBooking(BookingStatus.Pending);

        await Assert.ThrowsAsync<MechanicAssignmentException>(() =>
            _service.AssignMechanicAsync(new AssignMechanicRequestDto { BookingId = booking.Id, MechanicId = MechanicId }));
        Assert.Equal(BookingStatus.Pending, booking.Status);
    }

    [Fact]
    public async Task AssignMechanic_AllowsBackToBackJobs()
    {
        AddBooking(BookingStatus.Assigned, MechanicId, _tomorrow9.AddHours(-2), OtherCustomerId);
        var booking = AddBooking(BookingStatus.Pending);

        await _service.AssignMechanicAsync(new AssignMechanicRequestDto { BookingId = booking.Id, MechanicId = MechanicId });

        Assert.Equal(BookingStatus.Assigned, booking.Status);
    }

    [Fact]
    public async Task AssignMechanic_OnlyWorksForPendingBookings()
    {
        var booking = AddBooking(BookingStatus.InProgress, OtherMechanicId);

        await Assert.ThrowsAsync<MechanicAssignmentException>(() =>
            _service.AssignMechanicAsync(new AssignMechanicRequestDto { BookingId = booking.Id, MechanicId = MechanicId }));
    }

    // --- Doing the job ---

    [Fact]
    public async Task StartJob_CreatesAJobForTheAssignedMechanic()
    {
        var booking = AddBooking(BookingStatus.Assigned, MechanicId);

        await _service.StartJobAsync(booking.Id, MechanicId);

        Assert.Equal(BookingStatus.InProgress, booking.Status);
        var job = Assert.Single(_db.Jobs);
        Assert.Equal(MechanicId, job.MechanicId);
        Assert.Null(job.CompletionDate);
    }

    [Fact]
    public async Task StartJob_RefusesAMechanicWhoIsNotAssigned()
    {
        var booking = AddBooking(BookingStatus.Assigned, MechanicId);

        await Assert.ThrowsAsync<JobStartConditionException>(() => _service.StartJobAsync(booking.Id, OtherMechanicId));
        Assert.Equal(BookingStatus.Assigned, booking.Status);
    }

    private (Booking Booking, Job Job) BookingInProgress()
    {
        var booking = AddBooking(BookingStatus.InProgress, MechanicId);
        var job = new Job { Id = 1, BookingId = booking.Id, MechanicId = MechanicId, StartDate = DateTime.UtcNow, Description = "Started" };
        booking.Jobs = [job];
        _db.Jobs.Add(job);
        return (booking, job);
    }

    [Fact]
    public async Task FinishJob_TakesPartsFromStockAndInvoicesLabourPlusParts()
    {
        var (booking, job) = BookingInProgress();

        await _service.FinishJobAsync(booking.Id, MechanicId, new FinishJobRequestDto
        {
            Description = "Replaced front pads",
            LabourCost = 80m,
            UsedParts = [new UsedPartDto { PartId = 1, Quantity = 2 }],
        });

        Assert.Equal(BookingStatus.AwaitingPayment, booking.Status);
        Assert.Equal(1, _db.Parts[0].CurrentStockLevel);
        Assert.Equal(90m, job.PartsCost);
        Assert.NotNull(job.CompletionDate);
        var invoice = Assert.Single(_db.Invoices);
        Assert.Equal(170m, invoice.TotalCost);
        _email.Verify(e => e.SendJobCompletedAwaitingPaymentAsync("jamie@example.com", "Jamie", booking.Id, 170m), Times.Once);
    }

    [Fact]
    public async Task FinishJob_RefusesToUseMorePartsThanAreInStock()
    {
        var (booking, _) = BookingInProgress();

        await Assert.ThrowsAsync<InsufficientStockException>(() => _service.FinishJobAsync(booking.Id, MechanicId, new FinishJobRequestDto
        {
            Description = "Replaced front pads",
            LabourCost = 80m,
            UsedParts = [new UsedPartDto { PartId = 1, Quantity = 4 }],
        }));

        Assert.Equal(BookingStatus.InProgress, booking.Status);
        Assert.Equal(0, _db.SaveCount);
    }

    // --- Payment ---

    [Fact]
    public async Task MarkAsPaid_RecordsThePaymentAndSendsTheInvoice()
    {
        var (booking, job) = BookingInProgress();
        job.LabourCost = 80m;
        job.CompletionDate = DateTime.UtcNow;
        job.JobParts = [new JobPart { PartId = 1, Part = _db.Parts[0], QuantityUsed = 1 }];
        booking.Status = BookingStatus.AwaitingPayment;

        await _service.MarkAsPaidAsync(booking.Id, new MarkAsPaidRequestDto { PaymentNotes = "Card" }, performedByUserId: 1);

        Assert.Equal(BookingStatus.Paid, booking.Status);
        Assert.NotNull(booking.PaidAt);
        var payment = Assert.Single(_db.Payments);
        Assert.Equal(125m, payment.Amount);
        Assert.Equal("Manual (Card)", payment.Method);
        _email.Verify(e => e.SendPaymentConfirmationAsync("jamie@example.com", It.Is<InvoiceViewModel>(i => i.GrandTotalFormatted == "125.00")), Times.Once);
    }

    [Theory]
    [InlineData(BookingStatus.Pending)]
    [InlineData(BookingStatus.InProgress)]
    [InlineData(BookingStatus.Paid)]
    public async Task MarkAsPaid_OnlyWorksForBookingsAwaitingPayment(BookingStatus status)
    {
        var booking = AddBooking(status, MechanicId);

        await Assert.ThrowsAsync<BookingOperationException>(() =>
            _service.MarkAsPaidAsync(booking.Id, new MarkAsPaidRequestDto(), performedByUserId: 1));
        Assert.Empty(_db.Payments);
    }

    // --- Cancelling ---

    [Fact]
    public async Task CancelByCustomer_CancelsTheirOwnBookingAndRecordsWhy()
    {
        var booking = AddBooking(BookingStatus.Assigned, MechanicId);

        await _service.CancelBookingByCustomerAsync(booking.Id, CustomerId, new CancelBookingRequestDto { Reason = "Car sold last week" });

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.Equal("Customer: Car sold last week", booking.CancellationReason);
        Assert.Equal(CustomerId, booking.CancelledByUserId);
    }

    [Fact]
    public async Task CancelByCustomer_CannotCancelSomeoneElsesBooking()
    {
        var booking = AddBooking(BookingStatus.Pending, customerId: OtherCustomerId);

        await Assert.ThrowsAsync<BookingAccessException>(() =>
            _service.CancelBookingByCustomerAsync(booking.Id, CustomerId, new CancelBookingRequestDto { Reason = "Not my booking at all" }));
        Assert.Equal(BookingStatus.Pending, booking.Status);
    }

    [Fact]
    public async Task CancelByCustomer_CannotCancelAFinishedJob()
    {
        var booking = AddBooking(BookingStatus.AwaitingPayment, MechanicId);

        await Assert.ThrowsAsync<BookingCancellationNotAllowedException>(() =>
            _service.CancelBookingByCustomerAsync(booking.Id, CustomerId, new CancelBookingRequestDto { Reason = "Changed my mind" }));
    }

    // --- Searching ---

    private static ClaimsPrincipal UserInRole(int id, string role) =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, id.ToString()), new Claim(ClaimTypes.Role, role)], "test"));

    [Fact]
    public async Task Search_LimitsCustomersToTheirOwnBookings()
    {
        var bookings = new Mock<IBookingRepository>();
        bookings.Setup(r => r.SearchBookingsAsync(CustomerId, null, null, null, null, null)).ReturnsAsync([]).Verifiable();
        _db.Mock.Setup(u => u.Bookings).Returns(bookings.Object);

        await _service.SearchBookingsAsync(new BookingSearchRequestDto(), UserInRole(CustomerId, Roles.Customer));

        bookings.Verify();
    }

    [Fact]
    public async Task Search_RefusesACustomerAskingForAnotherCustomersBookings()
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.SearchBookingsAsync(new BookingSearchRequestDto { CustomerId = OtherCustomerId }, UserInRole(CustomerId, Roles.Customer)));
    }

    [Fact]
    public async Task Search_LimitsMechanicsToTheirOwnJobs()
    {
        var bookings = new Mock<IBookingRepository>();
        bookings.Setup(r => r.SearchBookingsAsync(null, null, MechanicId, null, null, BookingStatus.Assigned)).ReturnsAsync([]).Verifiable();
        _db.Mock.Setup(u => u.Bookings).Returns(bookings.Object);

        await _service.SearchBookingsAsync(new BookingSearchRequestDto { Status = "assigned" }, UserInRole(MechanicId, Roles.Mechanic));

        bookings.Verify();
    }

    [Fact]
    public async Task Search_RejectsAnUnknownStatus()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.SearchBookingsAsync(new BookingSearchRequestDto { Status = "Lost" }, UserInRole(1, Roles.Manager)));
    }
}
