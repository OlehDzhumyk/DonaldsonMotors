namespace DonaldsonMotors.API.Exceptions
{
    /// <summary>
    /// Base class for exceptions thrown by the AuthService.
    /// </summary>
    public class AuthServiceException : Exception
    {
        public AuthServiceException() { }
        public AuthServiceException(string message) : base(message) { }
        public AuthServiceException(string message, Exception innerException) : base(message, innerException) { }
    }


    /// <summary>
    /// Exception thrown when an attempt is made to register a user that already exists.
    /// Typically maps to an HTTP 409 Conflict response.
    /// </summary>
    public class UserAlreadyExistsException : AuthServiceException
    {
        public string Email { get; }

        public UserAlreadyExistsException(string email)
            : base($"User with email '{email}' already exists.")
        {
            Email = email;
        }

        public UserAlreadyExistsException(string email, string message)
            : base(message)
        {
            Email = email;
        }

        public UserAlreadyExistsException(string email, string message, Exception innerException)
            : base(message, innerException)
        {
            Email = email;
        }
    }

    /// <summary>
    /// Exception thrown when an invalid or unsupported role is specified during an operation (e.g., staff registration).
    /// Typically maps to an HTTP 400 Bad Request response.
    /// </summary>
    public class InvalidRoleException : AuthServiceException // Or directly System.ArgumentException
    {
        public string RoleName { get; }

        public InvalidRoleException(string roleName)
            : base($"The role '{roleName}' is invalid or not permitted for this operation.")
        {
            RoleName = roleName;
        }

        public InvalidRoleException(string roleName, string message)
            : base(message)
        {
            RoleName = roleName;
        }

        public InvalidRoleException(string roleName, string message, Exception innerException)
            : base(message, innerException)
        {
            RoleName = roleName;
        }
    }


    /// <summary>
    /// Exception thrown when login fails due to invalid credentials (email not found or password incorrect).
    /// Typically maps to an HTTP 401 Unauthorized response.
    /// </summary>
    public class InvalidCredentialsException : AuthServiceException // Or directly System.UnauthorizedAccessException
    {
        public InvalidCredentialsException()
            : base("Invalid email or password.") { }

        public InvalidCredentialsException(string message)
            : base(message) { }

        public InvalidCredentialsException(string message, Exception innerException)
            : base(message, innerException) { }
    }


    /// <summary>
    /// Exception thrown when user registration fails due to validation errors (e.g., password policy).
    /// Typically maps to an HTTP 400 Bad Request response.
    /// </summary>
    public class RegistrationValidationException : AuthServiceException
    {
        public IEnumerable<string> Errors { get; }

        public RegistrationValidationException(IEnumerable<string> errors)
            : base($"User registration failed: {string.Join(", ", errors)}")
        {
            Errors = errors ?? Enumerable.Empty<string>();
        }

        public RegistrationValidationException(string message, IEnumerable<string> errors)
            : base(message)
        {
            Errors = errors ?? Enumerable.Empty<string>();
        }

        public RegistrationValidationException(string message, IEnumerable<string> errors, Exception innerException)
            : base(message, innerException)
        {
            Errors = errors ?? Enumerable.Empty<string>();
        }
    }
}
