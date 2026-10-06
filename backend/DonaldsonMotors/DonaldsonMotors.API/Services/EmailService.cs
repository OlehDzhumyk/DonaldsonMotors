using DonaldsonMotors.API.Interfaces;
using DonaldsonMotors.API.Options;
using DonaldsonMotors.API.ViewModels.Emails;
using DonaldsonMotors.API.ViewModels.Invoice;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using RazorLight;

namespace DonaldsonMotors.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;
        private readonly IRazorLightEngine _razorLightEngine;

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            ILogger<EmailService> logger,
            IRazorLightEngine razorLightEngine)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
            _razorLightEngine = razorLightEngine;
        }

        private async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            if (string.IsNullOrWhiteSpace(_emailSettings.SmtpHost))
            {
                _logger.LogInformation("SMTP is not configured; skipping email to {ToEmail} with subject '{Subject}'", toEmail, subject);
                return;
            }

            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromAddress));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = subject;
                email.Body = new TextPart(TextFormat.Html) { Text = htmlMessage };

                using var smtp = new SmtpClient();
                var secureSocketOption = _emailSettings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
                if (_emailSettings.SmtpPort == 465)
                {
                    secureSocketOption = SecureSocketOptions.SslOnConnect;
                }

                _logger.LogDebug("Connecting to SMTP: {Host}:{Port} with SSL/TLS: {UseSsl}", _emailSettings.SmtpHost, _emailSettings.SmtpPort, secureSocketOption);
                await smtp.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, secureSocketOption);

                _logger.LogDebug("Authenticating SMTP user: {User}", _emailSettings.SmtpUser);
                if (!string.IsNullOrEmpty(_emailSettings.SmtpUser) && !string.IsNullOrEmpty(_emailSettings.SmtpPass))
                {
                    await smtp.AuthenticateAsync(_emailSettings.SmtpUser, _emailSettings.SmtpPass);
                }

                _logger.LogDebug("Sending email to {ToEmail}", toEmail);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
                _logger.LogInformation("Email sent successfully to {ToEmail} with subject '{Subject}'", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail} with subject '{Subject}'. Error: {ErrorMessage}", toEmail, subject, ex.Message);
            }
        }

        // Emails are a side effect: a broken template must not fail the request that triggered it
        private async Task SendTemplateAsync<TModel>(string toEmail, string subject, string templatePath, TModel model)
        {
            string htmlBody;
            try
            {
                htmlBody = await _razorLightEngine.CompileRenderAsync(templatePath, model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to render email template {TemplatePath} for {ToEmail}", templatePath, toEmail);
                return;
            }
            await SendEmailAsync(toEmail, subject, htmlBody);
        }

        // --- Existing email methods (SendWelcomeEmailAsync, SendBookingConfirmedAsync, etc.) ---

        public async Task SendWelcomeEmailAsync(string toEmail, string customerName)
        {
            var subject = "Welcome to Donaldson Motors!";
            var viewModel = new WelcomeEmailViewModel { CustomerName = customerName }; // Ensure BaseEmailViewModel properties are set if needed
            await SendTemplateAsync(toEmail, subject, "Templates/Emails/WelcomeEmail.cshtml", viewModel);
        }

        public async Task SendBookingConfirmedAsync(string toEmail, string customerName, DateTime bookingDate, int bookingId, string serviceTypeName, string vehicleRegistration)
        {
            var subject = "Your Donaldson Motors Booking Confirmation";
            var viewModel = new BookingConfirmationEmailViewModel
            {
                CustomerName = customerName,
                BookingId = bookingId,
                ServiceTypeName = serviceTypeName,
                VehicleRegistration = vehicleRegistration,
                BookingDateFormatted = bookingDate.ToString("dddd, dd MMMM yyyy 'at' HH:mm")
            };
            // Ensure BaseEmailViewModel properties are set if needed by _EmailLayout.cshtml
            viewModel.CompanyName = _emailSettings.FromName; // Or however you get it
            viewModel.CurrentYear = DateTime.UtcNow.Year;
            viewModel.CompanyContactEmail = "support@donaldsonmotors.com"; 
            viewModel.WebsiteUrl = _emailSettings.WebsiteUrl;


            await SendTemplateAsync(toEmail, subject, "Templates/Emails/BookingConfirmationEmail.cshtml", viewModel);
        }

        public async Task SendTechnicianAssignedAsync(string toEmail, string customerName, string mechanicName, DateTime bookingDate, int bookingId)
        {
            var subject = "Mechanic Assigned - Donaldson Motors Booking Update";
            var viewModel = new TechnicianAssignedEmailViewModel
            {
                CustomerName = customerName,
                MechanicName = mechanicName,
                BookingId = bookingId,
                BookingDateFormatted = bookingDate.ToString("dddd, dd MMMM yyyy 'at' HH:mm"),
                CompanyName = _emailSettings.FromName,
                CurrentYear = DateTime.UtcNow.Year,
                CompanyContactEmail = _emailSettings.FromAddress, 
                WebsiteUrl = _emailSettings.WebsiteUrl
            };
            // The path here matches the file name we defined
            await SendTemplateAsync(toEmail, subject, "Templates/Emails/TechnicianAssignedEmail.cshtml", viewModel);
        }

        public async Task SendJobCompletedAwaitingPaymentAsync(string toEmail, string customerName, int bookingId, decimal totalAmount)
        {
            var subject = "Service Completed - Donaldson Motors Booking Update";
            var viewModel = new JobCompletedEmailViewModel
            {
                CustomerName = customerName,
                BookingId = bookingId,
                TotalAmountFormatted = totalAmount.ToString("F2"),
                PaymentUrl = $"{_emailSettings.WebsiteUrl}/bookings/{bookingId}/pay", // Example, adjust as needed
                                                                                      // BaseEmailViewModel properties
                CompanyName = _emailSettings.FromName,
                CurrentYear = DateTime.UtcNow.Year,
                CompanyContactEmail = _emailSettings.FromAddress,
                WebsiteUrl = _emailSettings.WebsiteUrl
            };
            await SendTemplateAsync(toEmail, subject, "Templates/Emails/JobCompletedEmail.cshtml", viewModel);
        }

        public async Task SendBookingCancelledAsync(string toEmail, string customerName, int bookingId, DateTime bookingDate, string? cancellationReason)
        {
            var subject = "Booking Cancelled - Donaldson Motors";
            var viewModel = new BookingCancelledEmailViewModel
            {
                CustomerName = customerName,
                BookingId = bookingId,
                BookingDateFormatted = bookingDate.ToString("dddd, dd MMMM yyyy 'at' HH:mm"),
                CancellationReason = cancellationReason, // This will be used by the @if in the template
                                                         // BaseEmailViewModel properties
                CompanyName = _emailSettings.FromName,
                CurrentYear = DateTime.UtcNow.Year,
                CompanyContactEmail = _emailSettings.FromAddress,
                WebsiteUrl = _emailSettings.WebsiteUrl
            };
            await SendTemplateAsync(toEmail, subject, "Templates/Emails/BookingCancelledEmail.cshtml", viewModel);
        }



        public async Task SendPaymentConfirmationAsync(string toEmail, InvoiceViewModel invoiceViewModel)
        {
            var subject = $"Your Paid Invoice from Donaldson Motors - Booking #{invoiceViewModel.BookingId}";
            string htmlBody;
            try
            {
                htmlBody = await _razorLightEngine.CompileRenderAsync("Templates/Invoices/InvoiceTemplate.cshtml", invoiceViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to render InvoiceTemplate for email. BookingId: {BookingId}", invoiceViewModel.BookingId);
                // Fallback to a very simple confirmation if template rendering fails
                htmlBody = $"<h1>Payment Confirmed</h1><p>Dear {invoiceViewModel.CustomerName},</p><p>Your payment for booking ID {invoiceViewModel.BookingId} (Total: £{invoiceViewModel.GrandTotalFormatted}) has been confirmed.</p><p>Thank you!</p>";
            }
            await SendEmailAsync(toEmail, subject, htmlBody);
        }


    }
}