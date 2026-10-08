using Microsoft.Extensions.Configuration;
using RideAway.Application.IServices;
using Stripe;
using Stripe.Checkout;

namespace RideAway.Infrastructure.Payments
{
    public class StripePaymentService : IStripePaymentService
    {
        private readonly IConfiguration _config;
        private readonly string _successUrl;
        private readonly string _cancelUrl;

        public StripePaymentService(IConfiguration config)
        {
            _config = config;

            var secretKey = config["Stripe:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
                throw new InvalidOperationException("Stripe is not configured: set the 'Stripe:SecretKey' setting.");

            StripeConfiguration.ApiKey = secretKey;

            _successUrl = config["Stripe:SuccessUrl"] ?? "https://localhost:7039/payment/success";
            _cancelUrl = config["Stripe:CancelUrl"] ?? "https://localhost:7039/payment/cancel";
        }

        /// <summary>
        /// Creates a Stripe Checkout session. Creating a session does NOT mean the
        /// customer paid - the payment is only settled once Stripe sends a
        /// signature-verified <c>checkout.session.completed</c> webhook.
        /// </summary>
        public async Task<PaymentResult> CreatePaymentSession(decimal amount, string currency)
        {
            if (amount <= 0)
                throw new ArgumentException("Payment amount must be greater than zero.", nameof(amount));

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
                            // Round rather than truncate: ZAR is a 2-decimal currency and a
                            // truncating cast silently under-collects on every odd amount.
                            UnitAmount = (long)decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "RideAway Payment"
                            },
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = _successUrl,
                CancelUrl = _cancelUrl
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return new PaymentResult
            {
                Reference = session.Id,
                CheckoutUrl = session.Url,
                Success = false, // Not paid yet - awaiting the webhook.
                Message = "Payment session created"
            };
        }
    }
}
