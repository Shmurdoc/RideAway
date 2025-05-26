using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;
using RideAway.Application.IServices.INotification;

namespace RideAway.Infrastructure.Notifications
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var client = new SendGridClient(_config["SendGrid:ApiKey"]);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("no-reply@rideaway.com", "RideAway Support"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(toEmail));

            await client.SendEmailAsync(msg);
        }
    }
}
