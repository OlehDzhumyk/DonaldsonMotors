using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.DTOs.Auth;
using DonaldsonMotors.API.DTOs.Booking;
using DonaldsonMotors.API.DTOs.Part;
using DonaldsonMotors.API.DTOs.Schedule;
using DonaldsonMotors.API.DTOs.ServiceType;
using DonaldsonMotors.API.DTOs.Supplier;
using DonaldsonMotors.API.DTOs.User;
using DonaldsonMotors.API.DTOs.Vehicle;
using DonaldsonMotors.API.ViewModels.Invoice;
using System.Security.Claims;

namespace DonaldsonMotors.API.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user based on the provided DTO.
        /// Throws exceptions for existing users or validation errors.
        /// </summary>
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto);

        /// <summary>
        /// Authenticates a user and provides a JWT token.
        /// Throws UnauthorizedAccessException for invalid credentials.
        /// </summary>
        Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
    }


    public interface IUserService
    {
        // Existing Methods
        Task<UserProfileResponseDto> GetProfileAsync(int userId);
        Task<UserProfileResponseDto> UpdateProfileAsync(int userId, UpdateProfileRequestDto dto);
        Task<VehicleResponseDto> AddVehicleAsync(int userId, VehicleRequestDto dto);
        Task UpdateVehicleAsync(int userId, string registration, VehicleRequestDto dto);
        Task DeleteVehicleAsync(int userId, string registration);

        // New Methods
        /// <summary>
        /// Allows an administrator (Manager) to update another user's profile information.
        /// </summary>
        Task<UserProfileResponseDto> AdminUpdateUserAsync(int userIdToEdit, AdminUpdateUserRequestDto dto);

        /// <summary>
        /// Allows an authenticated user to change their own password.
        /// </summary>
        Task ChangePasswordAsync(int userId, ChangePasswordRequestDto dto);
    }

    public interface IPaymentService
    {
        /// <summary>
        /// Processes payment for the booking. Throws if payment fails.
        /// </summary>
        Task ProcessPaymentAsync(int bookingId, decimal amount);
    }


    public interface IServiceTypeService
    {
        Task<IEnumerable<ServiceTypeResponseDto>> GetAllAsync();
        Task<ServiceTypeResponseDto?> GetByIdAsync(int id);
        Task<ServiceTypeResponseDto> CreateAsync(CreateServiceTypeRequestDto dto);
        Task<ServiceTypeResponseDto?> UpdateAsync(int id, UpdateServiceTypeRequestDto dto);
        Task<bool> DeleteAsync(int id);
    }


    public interface IJwtService
    {
        Task<(string token, DateTime expiresAt)> GenerateTokenAsync(ApplicationUser user);
    }

    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string toEmail, string customerName);

        Task SendBookingConfirmedAsync(string toEmail,
                                       string customerName,
                                       DateTime bookingDate,
                                       int bookingId,
                                       string serviceTypeName,
                                       string vehicleRegistration);

        Task SendTechnicianAssignedAsync(string toEmail,
                                         string customerName,
                                         string mechanicName,
                                         DateTime bookingDate,
                                         int bookingId);

        Task SendJobCompletedAwaitingPaymentAsync(string toEmail,
                                                 string customerName,
                                                 int bookingId,
                                                 decimal totalAmount);

        // MODIFIED: Now accepts InvoiceViewModel
        Task SendPaymentConfirmationAsync(string toEmail, InvoiceViewModel invoiceViewModel);

        Task SendBookingCancelledAsync(string toEmail,
                                       string customerName,
                                       int bookingId,
                                       DateTime bookingDate,
                                       string? cancellationReason);
    }


    /// <summary>
    /// Service for managing suppliers.
    /// </summary>
    public interface ISupplierService
    {
        Task<IEnumerable<SupplierResponseDto>> GetAllAsync();
        Task<SupplierResponseDto?> GetByIdAsync(int id);
        Task<SupplierResponseDto> CreateAsync(CreateSupplierRequestDto dto);
        Task<SupplierResponseDto?> UpdateAsync(int id, UpdateSupplierRequestDto dto);
        Task<bool> DeleteAsync(int id);
    }


    /// <summary>
    /// Manages the entire lifecycle of a booking.
    /// </summary>
    public interface IBookingService
    {
        // === Customer Methods ===
        Task<BookingResponseDto> CreateBookingAsync(int customerId, CreateBookingRequestDto dto);
        Task<IEnumerable<BookingResponseDto>> GetMyBookingsAsync(int customerId);

        // === Manager Methods ===
        Task<IEnumerable<BookingResponseDto>> GetActiveBookingsForDashboardAsync();
        Task AssignMechanicAsync(AssignMechanicRequestDto dto);

        // === Mechanic Methods ===
        Task StartJobAsync(int bookingId, int mechanicId);
        Task FinishJobAsync(int bookingId, int mechanicId, FinishJobRequestDto dto);

        // <summary>
        /// Searches for bookings based on specified criteria.
        /// </summary>
        Task<IEnumerable<BookingResponseDto>> SearchBookingsAsync(BookingSearchRequestDto searchParameters, ClaimsPrincipal userPerformingSearch);

        Task MarkAsPaidAsync(int bookingId, MarkAsPaidRequestDto dto, int performedByUserId);

        Task CancelBookingByAdminAsync(int bookingId, int adminUserId, CancelBookingRequestDto dto);

        Task CancelBookingByCustomerAsync(int bookingId, int customerId, CancelBookingRequestDto dto);

    }


    public interface IScheduleService
    {
        // Manager-only methods
        Task UpdateWorkingHoursAsync(IEnumerable<WorkingDayDto> workingDaysDto);
        Task UpdateLunchBreakAsync(LunchBreakDto lunchBreakDto);
        Task<ScheduleExceptionResponseDto> AddScheduleExceptionAsync(CreateScheduleExceptionDto exceptionDto);
        Task DeleteScheduleExceptionAsync(int id);
        Task<IEnumerable<ScheduleExceptionResponseDto>> GetScheduleExceptionsAsync();

        // New public method for customers
        /// <summary>
        /// Calculates and returns a list of available booking slots within a given date range.
        /// </summary>
        /// <param name="startDate">The start date of the range to check.</param>
        /// <param name="endDate">The end date of the range to check.</param>
        /// <returns>A list of available UTC DateTimes for booking.</returns>
        Task<IEnumerable<DateTime>> GetAvailabilityAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets a list of all mechanics and their availability for a specific job slot.
        /// For manager use when assigning a mechanic to a booking.
        /// </summary>
        Task<IEnumerable<MechanicAvailabilityDto>> GetAvailableMechanicsForSlotAsync(DateTime slotStart, int serviceTypeId);

    }

        
    /// <summary>
    /// Service for managing parts and stock levels.
    /// </summary>
    public interface IPartService
    {
        Task<IEnumerable<PartResponseDto>> GetAllAsync(string? searchTerm, int? supplierId);
        Task<PartResponseDto?> GetByIdAsync(int id);
        Task<PartResponseDto> CreateAsync(CreatePartRequestDto dto);
        Task<PartResponseDto?> UpdateAsync(int id, UpdatePartRequestDto dto);
        Task<PartResponseDto?> UpdateStockLevelAsync(int id, UpdateStockLevelRequestDto dto);
        Task<bool> DeleteAsync(int id);
    }



    public interface IJobService
    {
        Task StartJobAsync(int jobId, int mechanicId);
        Task AddPartToJobAsync(int jobId, int partId, int quantity);
        Task FinishJobAsync(int jobId, string notes, decimal labourHours);
    }

}
