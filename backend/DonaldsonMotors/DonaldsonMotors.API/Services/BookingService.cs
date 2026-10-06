using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.DTOs.Booking;
using DonaldsonMotors.API.Interfaces;
using DonaldsonMotors.API.Exceptions;
using DonaldsonMotors.API.Mappers;
using System.Security.Claims;
using DonaldsonMotors.API.ViewModels.Invoice;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DonaldsonMotors.API.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ILogger<BookingService> _logger;

        public BookingService(IUnitOfWork unitOfWork, IEmailService emailService, ILogger<BookingService> logger)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _logger = logger;
        }

        // === Customer Methods ===

        public async Task<BookingResponseDto> CreateBookingAsync(int customerId, CreateBookingRequestDto dto)
        {
            _logger.LogInformation("Customer {CustomerId} attempting to create booking for vehicle {VehicleReg} at slot {SlotStart}",
                customerId, dto.VehicleRegistrationNumber, dto.SlotStart);

            // 1. Validate Slot Time
            if (dto.SlotStart < DateTime.UtcNow)
            {
                throw new SlotUnavailableException(dto.SlotStart, "Cannot book a slot in the past.");
            }
            var slotStartUtc = DateTime.SpecifyKind(dto.SlotStart, DateTimeKind.Utc);

            // 2. Validate Customer and Vehicle using the consolidated UserRepository
            var customer = await _unitOfWork.Users.GetCustomerByIdWithVehiclesAsync(customerId)
                ?? throw new UserProfileNotFoundException(customerId); // Customer specific profile not found

            var vehicle = customer.Vehicles?.FirstOrDefault(v => v.RegistrationNumber.Equals(dto.VehicleRegistrationNumber, StringComparison.OrdinalIgnoreCase));
            if (vehicle == null)
            {
                var anyVehicleWithReg = await _unitOfWork.Vehicles.GetByRegistrationAsync(dto.VehicleRegistrationNumber);
                if (anyVehicleWithReg != null)
                    throw new VehicleAccessDeniedException(dto.VehicleRegistrationNumber, customerId);
                else
                    throw new VehicleNotFoundException(dto.VehicleRegistrationNumber);
            }

            // 3. Validate Service Type
            var serviceType = await _unitOfWork.ServiceTypes.GetByIdAsync(dto.ServiceTypeId)
                ?? throw new ServiceTypeNotFoundException(dto.ServiceTypeId);

            // 4. Final check for slot availability (race condition for any booking)
            var existingBookingOnSlot = (await _unitOfWork.Bookings.FindAsync(b =>
                                            b.SlotStart == slotStartUtc &&
                                            b.Status != BookingStatus.Cancelled))
                                        .FirstOrDefault();
            if (existingBookingOnSlot != null)
                throw new SlotUnavailableException(slotStartUtc, "This time slot has just been booked. Please select another one.");

            var newBooking = new Booking
            {
                CustomerId = customerId, // This is correct as customerId is the ApplicationUser.Id
                VehicleRegistrationNumber = dto.VehicleRegistrationNumber,
                ServiceTypeId = dto.ServiceTypeId,
                SlotStart = slotStartUtc,
                Status = BookingStatus.Pending
            };

            await _unitOfWork.Bookings.AddAsync(newBooking);
            try
            {
                await _unitOfWork.CompleteAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                // Another customer took the slot between our check above and this save.
                throw new SlotUnavailableException(slotStartUtc, "This time slot has just been booked. Please select another one.");
            }

            _logger.LogInformation("Successfully created BookingId {BookingId} for CustomerId {CustomerId}", newBooking.Id, customerId);

            newBooking.Customer = customer; // Assign for potential use in mapper if it relies on navigation
            newBooking.ServiceType = serviceType;
            newBooking.Vehicle = vehicle;

            if (!string.IsNullOrEmpty(customer.Email))
            {
                await _emailService.SendBookingConfirmedAsync(
                    customer.Email,
                    customer.FullName,
                    newBooking.SlotStart,
                    newBooking.Id,
                    serviceType.Name,
                    newBooking.VehicleRegistrationNumber);
            }

            return newBooking.ToResponseDto();
        }

        public async Task<IEnumerable<BookingResponseDto>> GetMyBookingsAsync(int customerId)
        {
            _logger.LogInformation("Fetching bookings for customer {CustomerId}", customerId);
            // IBookingRepository.GetBookingsByCustomerIdAsync should .Include(b => b.ServiceType) and .Include(b => b.Vehicle)
            var bookings = await _unitOfWork.Bookings.GetBookingsByCustomerIdAsync(customerId);
            return bookings.Select(b => b.ToResponseDto());
        }

        /// <summary>
        /// Cancels a booking by a customer.
        /// </summary>
        public async Task CancelBookingByCustomerAsync(int bookingId, int customerId, CancelBookingRequestDto dto)
        {
            _logger.LogInformation("Customer {CustomerId} attempting to cancel BookingId {BookingId} with reason: {Reason}",
                customerId, bookingId, dto.Reason);

            // Fetch booking with details to get Customer info for email and for checks
            var booking = await _unitOfWork.Bookings.GetByIdWithDetailsAsync(bookingId)
                ?? throw new BookingNotFoundException(bookingId);

            if (booking.CustomerId != customerId)
            {
                _logger.LogWarning("Unauthorized attempt by CustomerId {AttemptingCustomerId} to cancel BookingId {BookingId} owned by CustomerId {ActualCustomerId}.",
                    customerId, bookingId, booking.CustomerId);
                throw new BookingAccessException(bookingId, customerId, "You are not authorized to cancel this booking.");
            }

            // Customers can cancel Pending, Assigned, or InProgress bookings
            if (booking.Status != BookingStatus.Pending &&
                booking.Status != BookingStatus.Assigned &&
                booking.Status != BookingStatus.InProgress) // Allowing cancellation even if InProgress, as per your last request
            {
                _logger.LogWarning("Customer {CustomerId} attempt to cancel BookingId {BookingId} with invalid status {Status}.",
                    customerId, bookingId, booking.Status);
                throw new BookingCancellationNotAllowedException(bookingId, $"Customer cannot cancel booking with status '{booking.Status}'. Allowed: Pending, Assigned, InProgress.");
            }

            booking.Status = BookingStatus.Cancelled;
            booking.CancellationReason = $"Customer: {dto.Reason}";
            booking.CancelledAt = DateTime.UtcNow;
            booking.CancelledByUserId = customerId;

            // _unitOfWork.Bookings.Update(booking); // EF Core tracks changes on fetched entity
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("BookingId {BookingId} was cancelled by CustomerId {CustomerId}.", bookingId, customerId);

            // Send cancellation email
            if (booking.Customer?.Email != null) 
            {
                await _emailService.SendBookingCancelledAsync(
                    booking.Customer.Email,
                    booking.Customer.FullName,
                    booking.Id,
                    booking.SlotStart,
                    booking.CancellationReason
                );
            }
            else
            {
                _logger.LogWarning("Could not send cancellation email for BookingId {BookingId}: Customer email or details missing.", bookingId);
            }
        }

        /// <summary>
        /// Cancels a booking by an administrator (Manager).
        /// </summary>
        public async Task CancelBookingByAdminAsync(int bookingId, int adminUserId, CancelBookingRequestDto dto)
        {
            _logger.LogInformation("Admin {AdminUserId} attempting to cancel BookingId {BookingId} with reason: {Reason}",
                adminUserId, bookingId, dto.Reason);

            var booking = await _unitOfWork.Bookings.GetByIdWithDetailsAsync(bookingId) // Includes Customer for email
                ?? throw new BookingNotFoundException(bookingId);

            // Managers can cancel Pending, Assigned, or InProgress bookings
            if (booking.Status != BookingStatus.Pending &&
                booking.Status != BookingStatus.Assigned &&
                booking.Status != BookingStatus.InProgress)
            {
                _logger.LogWarning("Admin {AdminUserId} attempt to cancel BookingId {BookingId} with invalid status {Status}.",
                    adminUserId, bookingId, booking.Status);
                throw new BookingCancellationNotAllowedException(bookingId, $"Admin cannot cancel booking with status '{booking.Status}'. Allowed: Pending, Assigned, InProgress.");
            }

            var adminUser = await _unitOfWork.Users.GetUserByIdAsync(adminUserId); // For logging clarity

            booking.Status = BookingStatus.Cancelled;
            booking.CancellationReason = $"Admin ({adminUser?.FullName ?? "ID: " + adminUserId}): {dto.Reason}";
            booking.CancelledAt = DateTime.UtcNow;
            booking.CancelledByUserId = adminUserId;

            // _unitOfWork.Bookings.Update(booking);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("BookingId {BookingId} was cancelled by AdminUserId {AdminUserId}.", bookingId, adminUserId);

            // Send cancellation email to the customer
            if (booking.Customer?.Email != null)
            {
                await _emailService.SendBookingCancelledAsync(
                    booking.Customer.Email,
                    booking.Customer.FullName,
                    booking.Id,
                    booking.SlotStart,
                    booking.CancellationReason
                );
            }
            else
            {
                _logger.LogWarning("Could not send cancellation email to customer for BookingId {BookingId}: Customer email or details missing.", bookingId);
            }
        }

        /// <summary>
        /// Marks a booking as paid by an authorized user (Manager/AccountsClerk).
        /// This will finalize the invoice details based on the completed job,
        /// create a Payment record, update the booking status, and send a confirmation email.
        /// </summary>
        public async Task MarkAsPaidAsync(int bookingId, MarkAsPaidRequestDto dto, int performedByUserId)
        {
            _logger.LogInformation("User {PerformedByUserId} attempting to mark BookingId {BookingId} as paid. Notes: {PaymentNotes}",
                performedByUserId, bookingId, dto.PaymentNotes);

            var booking = await _unitOfWork.Bookings.GetByIdWithDetailsAsync(bookingId)
                ?? throw new BookingNotFoundException(bookingId);

            if (booking.Status == BookingStatus.Paid)
            {
                throw new BookingOperationException($"Booking ID {bookingId} is already marked as paid.");
            }
            if (booking.Status != BookingStatus.AwaitingPayment)
            {
                throw new BookingOperationException($"Booking ID {bookingId} must be in 'AwaitingPayment' status to be marked as paid. Current status: '{booking.Status}'.");
            }

            var job = booking.Jobs?.FirstOrDefault(j => j.CompletionDate != null && j.MechanicId == booking.MechanicId);
            if (job == null)
            {
                throw new BookingOperationException($"No completed job directly associated with the assigned mechanic found for booking ID {bookingId}. Cannot finalize invoice or mark as paid.");
            }
            decimal finalLabourCost = job.LabourCost;
            decimal finalPartsCost = job.JobParts?.Sum(jp => jp.QuantityUsed * jp.UnitPrice) ?? 0m;


            var invoiceEntity = booking.Invoice;
            if (invoiceEntity == null)
            {
                invoiceEntity = (await _unitOfWork.Invoices.FindAsync(i => i.BookingId == bookingId)).FirstOrDefault();
                if (invoiceEntity == null)
                {
                    invoiceEntity = new Invoice { BookingId = bookingId };
                    await _unitOfWork.Invoices.AddAsync(invoiceEntity);
                    booking.Invoice = invoiceEntity;
                }
            }
            invoiceEntity.DateIssued = DateTime.UtcNow;
            invoiceEntity.TotalCost = finalLabourCost + finalPartsCost;

            var payment = new Payment
            {
                InvoiceId = invoiceEntity.Id,
                Amount = invoiceEntity.TotalCost,
                DatePaid = DateTime.UtcNow,
                Method = !string.IsNullOrWhiteSpace(dto.PaymentNotes) ? $"Manual ({dto.PaymentNotes})" : "Manual (Marked as Paid)"
            };
            await _unitOfWork.Payments.AddAsync(payment);

            booking.Status = BookingStatus.Paid;
            booking.PaidAt = DateTime.UtcNow;

            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("BookingId {BookingId} successfully marked as paid. PaymentId: {PaymentId}, InvoiceId: {InvoiceId}",
                bookingId, payment.Id, invoiceEntity.Id);

            // Prepare InvoiceViewModel for the email
            var partsForViewModel = new List<InvoicePartItemViewModel>();
            if (job.JobParts != null)
            {
                foreach (var jp in job.JobParts)
                {
                    if (jp.Part == null) continue; // Should be loaded
                    var partSubtotal = jp.UnitPrice * jp.QuantityUsed;
                    partsForViewModel.Add(new InvoicePartItemViewModel
                    {
                        PartName = jp.Part.Name,
                        QuantityUsed = jp.QuantityUsed,
                        UnitPriceFormatted = jp.UnitPrice.ToString("F2"),
                        SubtotalFormatted = partSubtotal.ToString("F2")
                    });
                }
            }

            var invoiceViewModel = new InvoiceViewModel
            {
                InvoiceId = invoiceEntity.Id.ToString(),
                BookingId = booking.Id,
                DateIssuedFormatted = invoiceEntity.DateIssued.ToString("MMMM dd, yyyy"),
                PaymentStatus = "Paid", // Explicitly Paid for this email

                CompanyName = "Donaldson Motors",
                CompanyAddressLine1 = "123 Service Lane",
                CompanyPostcode = "Glasgow, G1 2AB",
                CompanyEmail = "contact@donaldsonmotors.com",
                CompanyPhone = "0141 123 4567",

                CustomerName = booking.Customer?.FullName ?? "N/A",
                CustomerAddress = booking.Customer?.Address,
                CustomerEmail = booking.Customer?.Email ?? "N/A",
                CustomerPhone = booking.Customer?.PhoneNumber,

                JobDescription = job.Description ?? booking.ServiceType?.Name ?? "N/A",
                JobLabourCostFormatted = finalLabourCost.ToString("F2"),
                PartsUsed = partsForViewModel,
                TotalPartsCostFormatted = finalPartsCost.ToString("F2"),
                GrandTotalFormatted = invoiceEntity.TotalCost.ToString("F2"),
                CurrentYear = DateTime.UtcNow.Year
            };

            if (booking.Customer?.Email != null)
            {
                // Call the updated email service method
                await _emailService.SendPaymentConfirmationAsync(
                    booking.Customer.Email,
                    invoiceViewModel
                );
            }
            else
            {
                _logger.LogWarning("Could not send payment confirmation email for BookingId {BookingId}: Customer email missing.", bookingId);
            }
        }

        // === Manager Methods ===

        public async Task<IEnumerable<BookingResponseDto>> GetActiveBookingsForDashboardAsync()
        {
            _logger.LogInformation("Fetching active bookings for manager dashboard.");
            // IBookingRepository.GetDashboardBookingsAsync should .Include(b => b.ServiceType), .Include(b => b.Customer), .Include(b => b.Vehicle)
            var bookings = await _unitOfWork.Bookings.GetDashboardBookingsAsync();
            return bookings.Select(b => b.ToResponseDto());
        }

        public async Task AssignMechanicAsync(AssignMechanicRequestDto dto)
        {
            _logger.LogInformation("Manager assigning mechanic {MechanicId} to booking {BookingId}", dto.MechanicId, dto.BookingId);
            var bookingToAssign = await _unitOfWork.Bookings.GetByIdWithDetailsAsync(dto.BookingId) // Includes Customer & ServiceType
                ?? throw new BookingNotFoundException(dto.BookingId);

            if (bookingToAssign.Status != BookingStatus.Pending)
                throw new MechanicAssignmentException(bookingToAssign.Id, $"Booking must be in 'Pending' state to assign a mechanic. Current state: '{bookingToAssign.Status}'.", dto.MechanicId);

            // Use the consolidated UserRepository to get the Employee
            var mechanic = await _unitOfWork.Users.GetEmployeeByIdAsync(dto.MechanicId)
                ?? throw new EmployeeNotFoundException(dto.MechanicId, "Mechanic");
            // No need to cast if GetEmployeeByIdAsync returns Employee directly.

            double newBookingDurationHours = bookingToAssign.ServiceType?.DurationHours ?? 2.0;
            DateTime newBookingSlotStart = bookingToAssign.SlotStart;
            DateTime newBookingSlotEnd = newBookingSlotStart.AddHours(newBookingDurationHours);

            // Use the consolidated UserRepository to get mechanic's bookings for that date
            var mechanicBookingsOnDate = await _unitOfWork.Users.GetMechanicBookingsOnDateAsync(
                dto.MechanicId,
                newBookingSlotStart.Date // Pass the Date part as per IUserRepository
            );

            bool isMechanicActuallyAvailable = true;
            if (mechanicBookingsOnDate.Any())
            {
                foreach (var existingBooking in mechanicBookingsOnDate)
                {
                    if (existingBooking.Id == dto.BookingId) continue;

                    var existingBookingStart = existingBooking.SlotStart;
                    var existingBookingDuration = existingBooking.ServiceType?.DurationHours ?? 2.0;
                    var existingBookingEnd = existingBookingStart.AddHours(existingBookingDuration);

                    if (newBookingSlotStart < existingBookingEnd && newBookingSlotEnd > existingBookingStart)
                    {
                        isMechanicActuallyAvailable = false;
                        _logger.LogWarning("Mechanic {MechanicId} unavailable for BookingId {BookingId} due to conflict with their existing BookingId {ExistingBookingId} ({ExistingStart} - {ExistingEnd}) vs New Slot ({NewStart} - {NewEnd})",
                            dto.MechanicId, dto.BookingId, existingBooking.Id, existingBookingStart, existingBookingEnd, newBookingSlotStart, newBookingSlotEnd);
                        break;
                    }
                }
            }

            if (!isMechanicActuallyAvailable)
            {
                throw new MechanicAssignmentException(dto.BookingId, $"Mechanic {mechanic.FullName} (ID: {dto.MechanicId}) is not available at the requested time (Slot: {newBookingSlotStart:yyyy-MM-dd HH:mm} - {newBookingSlotEnd:HH:mm}) due to conflicting bookings.", dto.MechanicId);
            }

            bookingToAssign.MechanicId = mechanic.Id;
            bookingToAssign.Status = BookingStatus.Assigned;
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Successfully assigned mechanic {MechanicId} to booking {BookingId}", dto.MechanicId, dto.BookingId);

            if (bookingToAssign.Customer?.Email != null)
            {
                await _emailService.SendTechnicianAssignedAsync(
                    bookingToAssign.Customer.Email,
                    bookingToAssign.Customer.FullName,
                    mechanic.FullName,
                    bookingToAssign.SlotStart,
                    bookingToAssign.Id);
            }
        }

        // === Mechanic Methods ===

        public async Task StartJobAsync(int bookingId, int mechanicId)
        {
            _logger.LogInformation("Mechanic {MechanicId} starting job for booking {BookingId}", mechanicId, bookingId);
            var booking = await _unitOfWork.Bookings.GetByIdWithDetailsAsync(bookingId) // Ensure ServiceType is included
                ?? throw new BookingNotFoundException(bookingId);

            if (booking.MechanicId != mechanicId)
                throw new JobStartConditionException(bookingId, "You are not the assigned mechanic for this booking.", mechanicId);

            if (booking.Status != BookingStatus.Assigned)
                throw new JobStartConditionException(bookingId, $"Cannot start job. Booking status must be 'Assigned', but is '{booking.Status}'.", mechanicId);

            var existingActiveJob = (await _unitOfWork.Jobs.FindAsync(j => j.BookingId == bookingId && j.CompletionDate == null)).FirstOrDefault();
            if (existingActiveJob != null)
            {
                throw new JobStartConditionException(bookingId, "An active job already exists for this booking.", mechanicId);
            }

            var job = new Job
            {
                BookingId = bookingId,
                MechanicId = mechanicId,
                StartDate = DateTime.UtcNow,
                Description = $"Work started on {booking.ServiceType?.Name ?? "service"}"
            };
            await _unitOfWork.Jobs.AddAsync(job);
            booking.Status = BookingStatus.InProgress;
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Job {JobId} started for booking {BookingId}.", job.Id, bookingId);
        }

        public async Task FinishJobAsync(int bookingId, int mechanicId, FinishJobRequestDto dto)
        {
            _logger.LogInformation("Mechanic {MechanicId} finishing job {BookingId}", mechanicId, bookingId);

            var booking = await _unitOfWork.Bookings.GetByIdWithDetailsAsync(bookingId)
                ?? throw new BookingNotFoundException(bookingId);

            if (booking.MechanicId != mechanicId)
                throw new JobFinishConditionException(bookingId, "You are not the assigned mechanic for this booking.", mechanicId);

            if (booking.Status != BookingStatus.InProgress)
                throw new JobFinishConditionException(bookingId, $"Cannot finish job. Booking status must be 'InProgress', but is '{booking.Status}'.", mechanicId);

            var job = booking.Jobs?.FirstOrDefault(j => j.CompletionDate == null && j.MechanicId == mechanicId)
                ?? throw new JobFinishConditionException(bookingId, "No active job assigned to you found for this booking.", mechanicId);

            decimal calculatedPartsCost = 0m;
            if (dto.UsedParts != null && dto.UsedParts.Any())
            {
                // The same part listed twice becomes one line (JobPart is keyed by job + part).
                var usedParts = dto.UsedParts
                    .GroupBy(p => p.PartId)
                    .Select(g => (PartId: g.Key, Quantity: g.Sum(p => p.Quantity)));

                foreach (var (partId, quantity) in usedParts)
                {
                    var part = await _unitOfWork.Parts.GetByIdAsync(partId)
                        ?? throw new PartNotFoundException(partId);

                    if (part.CurrentStockLevel < quantity)
                        throw new InsufficientStockException(part.Id, part.Name, quantity, part.CurrentStockLevel);

                    part.CurrentStockLevel -= quantity;
                    calculatedPartsCost += part.Price * quantity;

                    await _unitOfWork.JobParts.AddAsync(new JobPart
                    {
                        JobId = job.Id,
                        PartId = part.Id,
                        QuantityUsed = quantity,
                        UnitPrice = part.Price,
                    });
                }
            }
            job.PartsCost = calculatedPartsCost;
            job.Description = dto.Description;
            job.LabourCost = dto.LabourCost;
            job.CompletionDate = DateTime.UtcNow;

            booking.Status = BookingStatus.AwaitingPayment;

            var invoice = booking.Invoice ?? (await _unitOfWork.Invoices.FindAsync(i => i.BookingId == bookingId)).FirstOrDefault();
            if (invoice == null)
            {
                invoice = new Invoice { BookingId = bookingId };
                await _unitOfWork.Invoices.AddAsync(invoice);
                booking.Invoice = invoice;
            }
            invoice.DateIssued = DateTime.UtcNow;
            invoice.TotalCost = job.LabourCost + job.PartsCost;

            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Job {JobId} finished for booking {BookingId}. Invoice {InvoiceId} updated/created. Total: {TotalCost}", job.Id, bookingId, invoice.Id, invoice.TotalCost);

            if (booking.Customer?.Email != null)
            {
                await _emailService.SendJobCompletedAwaitingPaymentAsync(
                   booking.Customer.Email,
                   booking.Customer.FullName,
                   bookingId,
                   invoice.TotalCost);
            }
        }

        public async Task<IEnumerable<BookingResponseDto>> SearchBookingsAsync(BookingSearchRequestDto searchParams, ClaimsPrincipal userPerformingSearch)
        {
            _logger.LogInformation("Searching bookings with criteria: CustomerId={CustomerId}, VehicleReg={VehicleReg}, MechanicId={MechanicId}, DateFrom={DateFrom}, DateTo={DateTo}, Status={Status}",
                searchParams.CustomerId, searchParams.VehicleRegistrationNumber, searchParams.MechanicId, searchParams.DateFrom, searchParams.DateTo, searchParams.Status);

            // Convert string status to BookingStatus enum if provided
            BookingStatus? bookingStatus = null;
            if (!string.IsNullOrWhiteSpace(searchParams.Status))
            {
                if (Enum.TryParse<BookingStatus>(searchParams.Status, true, out var parsedStatus))
                {
                    bookingStatus = parsedStatus;
                }
                else
                {
                    _logger.LogWarning("Invalid status string provided for booking search: {StatusString}", searchParams.Status);
                    // Optionally throw an ArgumentException or ignore the status filter
                    throw new ArgumentException($"Invalid booking status provided: {searchParams.Status}");
                }
            }


            int? effectiveCustomerId = searchParams.CustomerId;
            int? effectiveMechanicId = searchParams.MechanicId;

            if (userPerformingSearch.IsInRole(Roles.Customer))
            {
                var customerIdFromToken = int.Parse(userPerformingSearch.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (effectiveCustomerId.HasValue && effectiveCustomerId.Value != customerIdFromToken)
                {
                    // Customer trying to search for another customer's bookings
                    _logger.LogWarning("Customer {PerformingUserId} attempted to search bookings for another Customer {TargetCustomerId}", customerIdFromToken, effectiveCustomerId.Value);
                    throw new UnauthorizedAccessException("You can only search your own bookings.");
                }
                effectiveCustomerId = customerIdFromToken; // Enforce search for own bookings
            }
            else if (userPerformingSearch.IsInRole(Roles.Mechanic))
            {
                var mechanicIdFromToken = int.Parse(userPerformingSearch.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (effectiveMechanicId.HasValue && effectiveMechanicId.Value != mechanicIdFromToken)
                {
                    _logger.LogWarning("Mechanic {PerformingUserId} attempted to search bookings for another Mechanic {TargetMechanicId}", mechanicIdFromToken, effectiveMechanicId.Value);
                    throw new UnauthorizedAccessException("You can only search bookings assigned to you.");
                }
                effectiveMechanicId = mechanicIdFromToken; // Enforce search for own bookings
            }
            // Managers and AccountsClerks can search more broadly (no changes to effective IDs here)


            var bookings = await _unitOfWork.Bookings.SearchBookingsAsync(
                effectiveCustomerId,
                searchParams.VehicleRegistrationNumber,
                effectiveMechanicId,
                searchParams.DateFrom,
                searchParams.DateTo,
                bookingStatus
            );

            // The repository method SearchBookingsAsync should include all necessary related data
            // for ToBookingResponseDto to work correctly.
            return bookings.Select(b => b.ToResponseDto());
        }








    }
}