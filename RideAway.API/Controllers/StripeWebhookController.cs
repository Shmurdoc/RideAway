using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RideAway.Application.IServices;
using Stripe;

namespace RideAway.API.Controllers;

/// <summary>
/// Receives Stripe webhook events. This is the only path by which a card payment
/// becomes successful, so it verifies the Stripe signature on every request and
/// returns the same response regardless of whether the event was already processed
/// (Stripe retries anything that is not a 2xx).
/// </summary>
[ApiController]
[Route("api/webhooks")]
[AllowAnonymous]
public class StripeWebhookController : ControllerBase
{
    private const string SignatureHeader = "Stripe-Signature";

    private readonly IPaymentProcessingService _paymentProcessingService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeWebhookController> _logger;

    public StripeWebhookController(
        IPaymentProcessingService paymentProcessingService,
        IConfiguration configuration,
        ILogger<StripeWebhookController> logger)
    {
        _paymentProcessingService = paymentProcessingService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> HandleStripeWebhook()
    {
        var webhookSecret = _configuration["Stripe:WebhookSecret"];
        if (string.IsNullOrWhiteSpace(webhookSecret))
        {
            _logger.LogError("Stripe webhook received but 'Stripe:WebhookSecret' is not configured.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        var signature = Request.Headers[SignatureHeader].ToString();
        if (string.IsNullOrWhiteSpace(signature))
            return BadRequest("Missing Stripe-Signature header.");

        // Read the raw body: signature verification is computed over the exact bytes
        // Stripe sent, so any re-serialisation of a parsed model would break it.
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync();

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(payload, signature, webhookSecret);
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Rejected Stripe webhook with an invalid signature.");
            return BadRequest("Invalid signature.");
        }

        if (stripeEvent.Type == "checkout.session.completed")
        {
            var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
            if (session == null)
            {
                _logger.LogWarning("checkout.session.completed contained no session object.");
                return Ok();
            }

            if (session.PaymentStatus != "paid")
            {
                _logger.LogInformation("Checkout session {SessionId} is not paid yet (status {Status}).",
                    session.Id, session.PaymentStatus);
                return Ok();
            }

            var amount = (session.AmountTotal ?? 0) / 100m;
            var currency = session.Currency ?? string.Empty;

            await _paymentProcessingService.ConfirmPaymentAsync(session.Id, amount, currency);

            return Ok();
        }

        _logger.LogDebug("Ignoring Stripe event type {EventType}", stripeEvent.Type);
        return Ok();
    }
}
