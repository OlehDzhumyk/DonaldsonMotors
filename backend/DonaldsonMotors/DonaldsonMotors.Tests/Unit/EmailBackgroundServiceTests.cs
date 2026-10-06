using DonaldsonMotors.API.Services.Email;
using Microsoft.Extensions.Logging.Abstractions;

namespace DonaldsonMotors.Tests.Unit;

public class EmailBackgroundServiceTests
{
    private static readonly EmailMessage Welcome = new("jamie@example.com", "Welcome", "<p>Hi</p>");

    /// <summary>Fails the first <c>failures</c> sends, then records every message it sends.</summary>
    private sealed class FakeSender(int failures = 0) : IEmailSender
    {
        private int _calls;
        public List<EmailMessage> Sent { get; } = [];
        public int Calls => _calls;
        public TaskCompletionSource Done { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
        {
            if (Interlocked.Increment(ref _calls) <= failures) throw new IOException("SMTP is down");
            Sent.Add(message);
            Done.TrySetResult();
            return Task.CompletedTask;
        }
    }

    private static async Task RunUntil(IEmailQueue queue, IEmailSender sender, Func<Task> until)
    {
        var service = new EmailBackgroundService(queue, sender, NullLogger<EmailBackgroundService>.Instance, TimeSpan.Zero);
        await service.StartAsync(CancellationToken.None);
        await until().WaitAsync(TimeSpan.FromSeconds(5));
        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task SendsQueuedEmails()
    {
        var queue = new EmailQueue();
        var sender = new FakeSender();

        Assert.True(queue.TryEnqueue(Welcome));
        await RunUntil(queue, sender, () => sender.Done.Task);

        Assert.Equal(Welcome, Assert.Single(sender.Sent));
    }

    [Fact]
    public async Task RetriesAFailedSend()
    {
        var queue = new EmailQueue();
        var sender = new FakeSender(failures: EmailBackgroundService.MaxAttempts - 1);

        queue.TryEnqueue(Welcome);
        await RunUntil(queue, sender, () => sender.Done.Task);

        Assert.Equal(EmailBackgroundService.MaxAttempts, sender.Calls);
        Assert.Single(sender.Sent);
    }

    [Fact]
    public async Task GivesUpOnOneEmailAndCarriesOnWithTheNext()
    {
        var queue = new EmailQueue();
        var sender = new FakeSender(failures: EmailBackgroundService.MaxAttempts);
        var next = Welcome with { ToEmail = "priya@example.com" };

        queue.TryEnqueue(Welcome);
        queue.TryEnqueue(next);
        await RunUntil(queue, sender, () => sender.Done.Task);

        Assert.Equal(next, Assert.Single(sender.Sent));
    }

    [Fact]
    public void AFullQueueRefusesMoreEmails()
    {
        var queue = new EmailQueue();
        for (var i = 0; i < EmailQueue.Capacity; i++) Assert.True(queue.TryEnqueue(Welcome));

        Assert.False(queue.TryEnqueue(Welcome));
    }
}
