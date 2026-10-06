using System.Threading.Channels;

namespace DonaldsonMotors.API.Services.Email
{
    /// <summary>A rendered email waiting to be sent.</summary>
    public sealed record EmailMessage(string ToEmail, string Subject, string HtmlBody);

    public interface IEmailQueue
    {
        /// <summary>Queues an email without waiting for SMTP. Returns false if the queue is full.</summary>
        bool TryEnqueue(EmailMessage message);

        IAsyncEnumerable<EmailMessage> ReadAllAsync(CancellationToken cancellationToken);
    }

    /// <summary>
    /// In-memory queue between the request that triggers an email and <see cref="EmailBackgroundService"/>.
    /// Emails still queued when the app stops are lost; a database outbox would be the next step if that matters.
    /// </summary>
    public sealed class EmailQueue : IEmailQueue
    {
        public const int Capacity = 500;

        private readonly Channel<EmailMessage> _channel = Channel.CreateBounded<EmailMessage>(
            new BoundedChannelOptions(Capacity) { SingleReader = true, FullMode = BoundedChannelFullMode.Wait });

        public bool TryEnqueue(EmailMessage message) => _channel.Writer.TryWrite(message);

        public IAsyncEnumerable<EmailMessage> ReadAllAsync(CancellationToken cancellationToken) =>
            _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
