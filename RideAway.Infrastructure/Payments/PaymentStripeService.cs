using Microsoft.Extensions.Configuration;
using RideAway.Application.IServices;
using Stripe;
using Stripe.Checkout;

namespace RideAway.Infrastructure.Persistence.Repositories
{
    public class StripePaymentService : IStripePaymentService
    {
        private readonly IConfiguration _config;

        public StripePaymentService(IConfiguration config)
        {
            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
        }

        public async Task<PaymentResult> CreatePaymentSession(decimal amount, string currency)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = currency,
                    UnitAmount = (long)(amount * 100), // Stripe uses cents
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "RideAway Payment"
                    },
                },
                Quantity = 1
            }
        },
                Mode = "payment",
                SuccessUrl = "https://myapp-path.co.za/payment/success",
                CancelUrl = "https://myapp-path.co.za/payment/cancel"
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);
            return new PaymentResult
            {
                Reference = session.Id, // Stripe returns a session ID
                Success = true,
                Message = "Payment session created"
            };
        }

    }
}
