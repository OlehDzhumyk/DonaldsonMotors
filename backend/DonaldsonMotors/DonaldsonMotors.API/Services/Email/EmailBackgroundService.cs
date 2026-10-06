namespace DonaldsonMotors.API.Services.Email
{
    /// <summary>
    /// Sends queued emails one by one, so API requests never wait for the SMTP server.
    /// A failed send is retried a few times with a growing delay, then logged and dropped.
    /// </summary>
    public sealed class EmailBackgroundService(
        IEmailQueue queue,
        IEmailSender sender,
        ILogger<EmailBackgroundService> logger,
        TimeSpan? retryDelay = null) : BackgroundService
    {
        public const int MaxAttempts = 3;

        private readonly TimeSpan _retryDelay = retryDelay ?? TimeSpan.FromSeconds(5);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await foreach (var message in queue.ReadAllAsync(stoppingToken))
                {
                    await SendWithRetriesAsync(message, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // App is shutting down.
            }
        }

        private async Task SendWithRetriesAsync(EmailMessage message, CancellationToken stoppingToken)
        {
            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    await sender.SendAsync(message, stoppingToken);
                    return;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    if (attempt == MaxAttempts)
                    {
                        logger.LogError(ex, "Giving up on email to {ToEmail} ('{Subject}') after {Attempts} attempts",
                            message.ToEmail, message.Subject, attempt);
                        return;
                    }
                    logger.LogWarning(ex, "Email to {ToEmail} failed (attempt {Attempt}), retrying", message.ToEmail, attempt);
                    await Task.Delay(_retryDelay * attempt, stoppingToken);
                }
            }
        }
    }
}
