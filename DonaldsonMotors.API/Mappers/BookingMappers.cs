// BookingMapperExtensions.cs
using DonaldsonMotors.API.Domain.Models;
using DonaldsonMotors.API.DTOs.Booking;
using DonaldsonMotors.API.Data.Entities;


namespace DonaldsonMotors.API.Mappers
{


public static class BookingMapperExtensions
{
    // From DTO to Domain Model

    public static Booking ToDomainModel(this CreateBookingRequestDto dto) => new()
    {
        BookingDate = dto.BookingDate,
        Status = "Pending", // Initial status for a new booking
        // We don't have Customer or Vehicle objects directly from this DTO,
        // these will likely be fetched from the database based on IDs later.
        // The VehicleRegistration is available, which is good for identification.
    };

    public static Booking ToDomainModel(this UpdateBookingRequestDto dto) => new()
    {
        BookingDate = dto.BookingDate ?? DateTime.MinValue, // Handle nullable DateTime
        Status = dto.Status ?? string.Empty, // Handle nullable string
        // Similar to Create, we don't directly map Vehicle or Customer here.
    };

    // From Domain Model to DTO

    public static BookingResponseDto ToResponseDto(this Booking booking) => new()
    {
        Id = booking.Id,
        CustomerId = booking.Customer?.Id ?? 0, // Handle potential null Customer
        VehicleRegistration = booking.Vehicle?.Registration ?? string.Empty, // Handle potential null Vehicle
        BookingDate = booking.BookingDate,
        Status = booking.Status
    };

    // From Domain Model to Entity

    public static BookingEntity ToEntity(this Booking booking) => new()
    {
        Id = booking.Id,
        CustomerId = booking.Customer?.Id ?? 0, // Ensure CustomerId is set
        VehicleRegistration = booking.Vehicle?.Registration ?? string.Empty, // Ensure VehicleRegistration is set
        BookingDate = booking.BookingDate,
        Status = booking.Status
    };

    // From Entity to Domain Model

    public static Booking ToDomainModel(this BookingEntity entity) => new()
    {
        Id = entity.Id,
        BookingDate = entity.BookingDate,
        Status = entity.Status,
        // Navigation properties - ensure these are loaded if needed
        Customer = entity.Customer.ToDomainModel(),
        Vehicle = entity.Vehicle.ToDomainModel(),
        Jobs = entity.Jobs?.Select(j => j.ToDomainModel()).ToList() ?? new List<Job>(),
        Invoice = entity.Invoice?.ToDomainModel()
    };

    }

}