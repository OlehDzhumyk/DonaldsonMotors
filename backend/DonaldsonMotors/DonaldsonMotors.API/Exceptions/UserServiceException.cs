namespace DonaldsonMotors.API.Exceptions
{
    public class UserServiceException : Exception
    {
        public UserServiceException() { }
        public UserServiceException(string message) : base(message) { }
        public UserServiceException(string message, Exception innerException) : base(message, innerException) { }
    }



    public class UserProfileNotFoundException : UserServiceException
    {
        public int UserId { get; }

        public UserProfileNotFoundException(int userId)
            : base($"User profile with ID {userId} not found.")
        {
            UserId = userId;
        }

        public UserProfileNotFoundException(int userId, string message) : base(message)
        {
            UserId = userId;
        }
    }


    public class ProfileUpdateFailedException : UserServiceException
    {
        public IEnumerable<string> Errors { get; }

        public ProfileUpdateFailedException(IEnumerable<string> errors)
            : base($"Profile update failed: {string.Join(", ", errors)}")
        {
            Errors = errors ?? Enumerable.Empty<string>();
        }

        public ProfileUpdateFailedException(string message, IEnumerable<string> errors)
            : base(message)
        {
            Errors = errors ?? Enumerable.Empty<string>();
        }
    }



    public class VehicleAlreadyExistsException : UserServiceException
    {
        public string RegistrationNumber { get; }

        public VehicleAlreadyExistsException(string registrationNumber)
            : base($"Vehicle with registration number '{registrationNumber}' already exists.")
        {
            RegistrationNumber = registrationNumber;
        }
    }




}