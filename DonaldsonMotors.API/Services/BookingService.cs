using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.DTOs.Booking;
using DonaldsonMotors.API.Interfaces;

namespace DonaldsonMotors.API.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BookingService> _logger;

        public BookingService(IUnitOfWork unitOfWork, ILogger<BookingService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // ====================================================================
        // CUSTOMER-FACING METHODS
        // ====================================================================

        public async Task<BookingResponseDto> CreateBookingAsync(int customerId, CreateBookingRequestDto dto)
        {
            _logger.LogInformation("Attempting to create booking for CustomerId {CustomerId}", customerId);

            var vehicle = await _unitOfWork.Vehicles.GetByRegistrationAsync(dto.VehicleRegistrationNumber)
                ?? throw new KeyNotFoundException("Vehicle not found.");
            if (vehicle.OwnerId != customerId)
                throw new UnauthorizedAccessException("Vehicle does not belong to the current user.");

            var serviceType = await _unitOfWork.ServiceTypes.GetByIdAsync(dto.ServiceTypeId)
                ?? throw new KeyNotFoundException("Invalid Service Type ID.");

            var existingBooking = (await _unitOfWork.Bookings.FindAsync(b => b.SlotStart == dto.SlotStart)).FirstOrDefault();
            if (existingBooking != null)
                throw new InvalidOperationException("This time slot has just been booked. Please select another one.");

            var newBooking = new Booking
            {
                CustomerId = customerId,
                VehicleRegistrationNumber = dto.VehicleRegistrationNumber,
                ServiceTypeId = dto.ServiceTypeId,
                SlotStart = dto.SlotStart,
                Status = BookingStatus.Pending
            };

            await _unitOfWork.Bookings.AddAsync(newBooking);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Successfully created BookingId {BookingId}", newBooking.Id);
            return MapToBookingResponseDto(newBooking, serviceType);
        }

        public async Task<IEnumerable<BookingResponseDto>> GetMyBookingsAsync(int customerId)
        {
            _logger.LogInformation("Fetching bookings for CustomerId {CustomerId}", customerId);
            var bookings = await _unitOfWork.Bookings.GetBookingsByCustomerIdAsync(customerId);
            return bookings.Select(b => MapToBookingResponseDto(b, b.ServiceType));
        }

        public async Task CancelBookingAsync(int bookingId, int customerId)
        {
            _logger.LogInformation("Customer {CustomerId} attempting to cancel BookingId {BookingId}", customerId, bookingId);
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId)
                ?? throw new KeyNotFoundException("Booking not found.");

            if (booking.CustomerId != customerId)
                throw new UnauthorizedAccessException("You are not authorized to cancel this booking.");

            if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Assigned)
                throw new InvalidOperationException("Only pending or assigned bookings can be cancelled.");

            booking.Status = BookingStatus.Cancelled;
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("BookingId {BookingId} was cancelled.", bookingId);
        }

        // ====================================================================
        // MANAGER-FACING METHODS
        // ====================================================================

        public async Task<IEnumerable<BookingResponseDto>> GetActiveBookingsForDashboardAsync()
        {
            _logger.LogInformation("Fetching active bookings for manager dashboard.");
            var bookings = await _unitOfWork.Bookings.GetDashboardBookingsAsync();
            // This mapping could be more detailed if the dashboard needs more info
            return bookings.Select(b => MapToBookingResponseDto(b, b.ServiceType));
        }

        public async Task AssignMechanicAsync(AssignMechanicRequestDto dto)
        {
            _logger.LogInformation("Attempting to assign MechanicId {MechanicId} to BookingId {BookingId}", dto.MechanicId, dto.BookingId);

            var booking = await _unitOfWork.Bookings.GetByIdAsync(dto.BookingId)
                ?? throw new KeyNotFoundException("Booking not found.");

            if (booking.Status != BookingStatus.Pending)
                throw new InvalidOperationException("A mechanic can only be assigned to a 'Pending' booking.");

            var mechanic = await _unitOfWork.Employees.GetByIdAsync(dto.MechanicId)
                ?? throw new KeyNotFoundException("Mechanic (Employee) not found.");

            var isAvailable = await _unitOfWork.Employees.IsMechanicAvailableAsync(dto.MechanicId, booking.SlotStart, 2);
            if (!isAvailable)
                throw new InvalidOperationException($"Mechanic {mechanic.FullName} is not available at the requested time.");

            booking.MechanicId = dto.MechanicId;
            booking.Status = BookingStatus.Assigned;
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Successfully assigned mechanic to BookingId {BookingId}", dto.BookingId);
        }

        // ====================================================================
        // MECHANIC-FACING METHODS
        // ====================================================================

        public async Task StartJobAsync(int bookingId, int mechanicId)
        {
            _logger.LogInformation("Mechanic {MechanicId} attempting to start job for BookingId {BookingId}", mechanicId, bookingId);
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId)
                ?? throw new KeyNotFoundException("Booking not found.");

            if (booking.MechanicId != mechanicId)
                throw new UnauthorizedAccessException("You are not the assigned mechanic for this job.");

            if (booking.Status != BookingStatus.Assigned)
                throw new InvalidOperationException("Job can only be started if the booking status is 'Assigned'.");

            booking.Status = BookingStatus.InProgress;
            // Create a corresponding Job record to track work details
            var newJob = new Job { BookingId = bookingId, MechanicId = mechanicId, StartDate = DateTime.UtcNow, Description = "Work in progress..." };
            await _unitOfWork.Jobs.AddAsync(newJob);

            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Job started for BookingId {BookingId}", bookingId);
        }

        public async Task FinishJobAsync(int bookingId, int mechanicId, FinishJobRequestDto dto)
        {
            _logger.LogInformation("Mechanic {MechanicId} finishing job for BookingId {BookingId}", mechanicId, bookingId);

            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId)
                ?? throw new KeyNotFoundException("Booking not found.");

            if (booking.MechanicId != mechanicId)
                throw new UnauthorizedAccessException("You are not the assigned mechanic for this job.");

            var job = (await _unitOfWork.Jobs.FindAsync(j => j.BookingId == bookingId)).FirstOrDefault()
                ?? throw new InvalidOperationException("No active job found for this booking. Has the job been started?");

            decimal totalPartsCost = 0m;
            foreach (var partDto in dto.UsedParts)
            {
                var part = await _unitOfWork.Parts.GetByIdAsync(partDto.PartId)
                    ?? throw new KeyNotFoundException($"Part with ID {partDto.PartId} not found.");

                if (part.CurrentStockLevel < partDto.Quantity)
                    throw new InvalidOperationException($"Not enough stock for part '{part.Name}'. Required: {partDto.Quantity}, Available: {part.CurrentStockLevel}.");

                part.CurrentStockLevel -= partDto.Quantity; // Decrement stock
                totalPartsCost += part.Price * partDto.Quantity;

                await _unitOfWork.JobParts.AddAsync(new JobPart { JobId = job.Id, PartId = part.Id, QuantityUsed = partDto.Quantity });
            }

            // Update job details
            job.Description = dto.Description;
            job.LabourCost = dto.LabourCost;
            job.PartsCost = totalPartsCost;
            job.CompletionDate = DateTime.UtcNow;

            // Update booking status
            booking.Status = BookingStatus.AwaitingPayment;

            await _unitOfWork.CompleteAsync(); // All changes (stock, job, booking) saved in one transaction
            _logger.LogInformation("Job finished for BookingId {BookingId}", bookingId);
        }

        // ====================================================================
        // PRIVATE HELPERS
        // ====================================================================

        private BookingResponseDto MapToBookingResponseDto(Booking booking, ServiceType? serviceType)
        {
            return new BookingResponseDto
            {
                Id = booking.Id,
                SlotStart = booking.SlotStart,
                Status = booking.Status.ToString(),
                VehicleRegistrationNumber = booking.VehicleRegistrationNumber,
                ServiceTypeName = serviceType?.Name ?? "N/A",
                ServiceTypePrice = serviceType?.Price ?? 0m
            };
        }
    }
}