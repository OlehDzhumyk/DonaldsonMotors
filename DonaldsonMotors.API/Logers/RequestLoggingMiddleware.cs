namespace DonaldsonMotors.API.Logers
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            // Логування вхідного запиту
            _logger.LogInformation("Incoming request: {Method} {Path}, Authenticated: {IsAuth}, User: {User}",
                context.Request.Method,
                context.Request.Path,
                context.User.Identity?.IsAuthenticated,
                context.User.Identity?.Name ?? "Anonymous");

            try
            {
                await _next(context);

                _logger.LogInformation("Response: {StatusCode} {Path}",
                    context.Response.StatusCode,
                    context.Request.Path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception while processing request {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);
                throw; // перезапустити pipeline (виняток має обробити інший middleware або викликатиме 500)
            }
        }
    }

}
