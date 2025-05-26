using Microsoft.Extensions.Configuration;
using RideAway.Application.IServices.INotification;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace RideAway.Infrastructure.Notifications
{
    public class SmsService : ISmsService
    {
        public SmsService(IConfiguration config)
        {
            TwilioClient.Init(config["Twilio:AccountSid"], config["Twilio:AuthToken"]);
        }

        public void SendSms(string phoneNumber, string message)
        {
            MessageResource.Create(
                body: message,
                from: new Twilio.Types.PhoneNumber("+1234567890"),
                to: new Twilio.Types.PhoneNumber(phoneNumber)
            );
        }
    }
}
