namespace DonaldsonMotors.API.Exceptions
{
    /// <summary>
    /// Base class for exceptions thrown by the BookingService or related operations.
    /// </summary>
    public class BookingOperationException : Exception
    {
        public BookingOperationException() { }
        public BookingOperationException(string message) : base(message) { }
        public BookingOperationException(string message, Exception innerException) : base(message, innerException) { }
    }


    public class BookingAccessException : BookingOperationException
    {
        public int BookingId { get; }
        public int UserId { get; }
        public BookingAccessException(int bookingId, int userId, string message) : base(message)
        {
            BookingId = bookingId;
            UserId = userId;
        }
    }


    public class BookingNotFoundException : BookingOperationException
    {
        public int BookingId { get; }
        public BookingNotFoundException(int bookingId)
            : base($"Booking with ID {bookingId} not found.")
        {
            BookingId = bookingId;
        }
    }


    public class VehicleNotFoundException : BookingOperationException
    {
        public string RegistrationNumber { get; }
        public VehicleNotFoundException(string registrationNumber)
            : base($"Vehicle with registration '{registrationNumber}' not found.")
        {
            RegistrationNumber = registrationNumber;
        }
    }


    public class VehicleAccessDeniedException : BookingOperationException
    {
        public string RegistrationNumber { get; }
        public int CustomerId { get; }
        public VehicleAccessDeniedException(string registrationNumber, int customerId)
            : base($"Customer ID {customerId} is not authorized to use vehicle '{registrationNumber}'.")
        {
            RegistrationNumber = registrationNumber;
            CustomerId = customerId;
        }
    }



    public class ServiceTypeNotFoundException : BookingOperationException
    {
        public int ServiceTypeId { get; }
        public ServiceTypeNotFoundException(int serviceTypeId)
            : base($"ServiceType with ID {serviceTypeId} not found.")
        {
            ServiceTypeId = serviceTypeId;
        }
    }


    public class SlotUnavailableException : BookingOperationException
    {
        public DateTime SlotStart { get; }
        public SlotUnavailableException(DateTime slotStart, string reason = "The selected time slot is no longer available.")
            : base(reason)
        {
            SlotStart = slotStart;
        }
    }



    public class BookingCancellationNotAllowedException : BookingOperationException
    {
        public int BookingId { get; }
        public BookingCancellationNotAllowedException(int bookingId, string reason)
            : base($"Cannot cancel booking ID {bookingId}: {reason}")
        {
            BookingId = bookingId;
        }
    }


    public class EmployeeNotFoundException : KeyNotFoundException
    {
        public int EmployeeId { get; }
        public string? Role { get; }
        public EmployeeNotFoundException(int employeeId, string? role = null)
            : base($"Employee{(role != null ? $" with role '{role}'" : "")} and ID {employeeId} not found.")
        {
            EmployeeId = employeeId;
            Role = role;
        }
    }


    public class MechanicAssignmentException : BookingOperationException
    {
        public int BookingId { get; }
        public int? MechanicId { get; }
        public MechanicAssignmentException(int bookingId, string reason, int? mechanicId = null)
            : base($"Failed to assign mechanic to booking ID {bookingId}: {reason}")
        {
            BookingId = bookingId;
            MechanicId = mechanicId;
        }
    }



    public class JobOperationException : BookingOperationException
    {
        public int BookingId { get; }
        public int? MechanicId { get; }
        public JobOperationException(int bookingId, string message, int? mechanicId = null) : base(message)
        {
            BookingId = bookingId;
            MechanicId = mechanicId;
        }
        public JobOperationException(int bookingId, string message, Exception innerException, int? mechanicId = null) : base(message, innerException)
        {
            BookingId = bookingId;
            MechanicId = mechanicId;
        }
    }


    public class JobStartConditionException : JobOperationException
    {
        public JobStartConditionException(int bookingId, string reason, int? mechanicId = null)
            : base(bookingId, $"Cannot start job for booking ID {bookingId}: {reason}", mechanicId) { }
    }


    public class JobFinishConditionException : JobOperationException
    {
        public JobFinishConditionException(int bookingId, string reason, int? mechanicId = null)
            : base(bookingId, $"Cannot finish job for booking ID {bookingId}: {reason}", mechanicId) { }
    }


    public class PartNotFoundException : KeyNotFoundException
    {
        public int PartId { get; }
        public PartNotFoundException(int partId)
            : base($"Part with ID {partId} not found.")
        {
            PartId = partId;
        }
    }


    public class InsufficientStockException : BookingOperationException
    {
        public int PartId { get; }
        public string PartName { get; }
        public int RequestedQuantity { get; }
        public int AvailableStock { get; }

        public InsufficientStockException(int partId, string partName, int requestedQuantity, int availableStock)
            : base($"Not enough stock for part '{partName}' (ID: {partId}). Requested: {requestedQuantity}, Available: {availableStock}.")
        {
            PartId = partId;
            PartName = partName;
            RequestedQuantity = requestedQuantity;
            AvailableStock = availableStock;
        }
    }



}
