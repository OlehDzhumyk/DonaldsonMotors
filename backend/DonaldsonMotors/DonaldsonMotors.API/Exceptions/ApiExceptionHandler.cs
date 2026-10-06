using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DonaldsonMotors.API.Exceptions
{
    /// <summary>
    /// Turns exceptions from the services into ProblemDetails responses, so controllers
    /// don't need their own try/catch. Anything not listed here is a 500 and gets logged.
    /// </summary>
    public sealed class ApiExceptionHandler(IProblemDetailsService problemDetails, ILogger<ApiExceptionHandler> logger)
        : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            var status = StatusFor(exception);
            var problem = new ProblemDetails { Status = status };

            if (status == StatusCodes.Status500InternalServerError)
            {
                logger.LogError(exception, "Unhandled error on {Method} {Path}", context.Request.Method, context.Request.Path);
                problem.Detail = "An unexpected error occurred.";
            }
            else
            {
                problem.Detail = exception.Message;
                var errors = exception switch
                {
                    RegistrationValidationException e => e.Errors,
                    ProfileUpdateFailedException e => e.Errors,
                    _ => null,
                };
                if (errors != null) problem.Extensions["errors"] = errors.ToArray();
            }

            context.Response.StatusCode = status;
            return await problemDetails.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = problem,
                Exception = exception,
            });
        }

        // Order matters: more specific types first.
        private static int StatusFor(Exception exception) => exception switch
        {
            InvalidCredentialsException => StatusCodes.Status401Unauthorized,

            BookingAccessException or VehicleAccessDeniedException or UnauthorizedAccessException
                => StatusCodes.Status403Forbidden,

            KeyNotFoundException or BookingNotFoundException or VehicleNotFoundException
                or ServiceTypeNotFoundException or UserProfileNotFoundException
                => StatusCodes.Status404NotFound,

            SlotUnavailableException or InsufficientStockException or ConcurrencyConflictException
                or DuplicateResourceException or ResourceInUseException
                or UserAlreadyExistsException or VehicleAlreadyExistsException
                => StatusCodes.Status409Conflict,

            BookingOperationException or AuthServiceException or UserServiceException
                or ArgumentException or InvalidOperationException
                => StatusCodes.Status400BadRequest,

            _ => StatusCodes.Status500InternalServerError,
        };
    }
}
