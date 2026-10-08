using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RideAway.Application.DTOs;
using RideAway.Application.IRepositories;
using RideAway.Application.IServices;
using RideAway.Domain.Entities;
using RideAway.Domain.Entities.Enum;
using RideAway.Domain.Exceptions;
using RideAway.Domain.Value_Object;

namespace RideAway.Application.Services
{
    public class PaymentProcessingService : IPaymentProcessingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStripePaymentService _stripeService;
        private readonly ILogger<PaymentProcessingService> _logger;
        private readonly string _currency;

        public PaymentProcessingService(
            IUnitOfWork unitOfWork,
            IStripePaymentService stripeService,
            ILogger<PaymentProcessingService> logger,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _stripeService = stripeService;
            _logger = logger;
            _currency = configuration["Stripe:Currency"] ?? "ZAR";
        }

        /// <summary>
        /// Creates the single pending payment for a ride. Settlement happens later:
        /// <see cref="ConfirmPaymentAsync"/> (Stripe webhook) or
        /// <see cref="ConfirmCashCollectionAsync"/> (driver confirms cash).
        /// </summary>
        public async Task<PaymentResultDTO> CreatePaymentAsync(Guid rideId, Guid userId, decimal amount, PaymentMethod method)
        {
            var existing = await _unitOfWork.PaymentRepository.Get(p => p.RideId == rideId && p.Status == PaymentStatus.Completed);
            if (existing != null)
                throw new PaymentProcessingException("This ride has already been paid.");

            var payment = Payment.CreatePending(rideId, userId, amount, method);

            switch (method)
            {
                case PaymentMethod.cash:
                    // Cash is never auto-successful. The assigned driver confirms
                    // collection, which is what settles the ride.
                    break;

                case PaymentMethod.card:
                case PaymentMethod.stripe:
                    if (_stripeService == null)
                        throw new InvalidOperationException("Stripe service is not configured.");

                    var session = await _stripeService.CreatePaymentSession(amount, _currency);
                    payment.TransactionReference = session.Reference;
                    break;

                default:
                    throw new PaymentProcessingException($"Payment method {method} is not supported.");
            }

            await _unitOfWork.PaymentRepository.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Created pending payment {PaymentId} for ride {RideId}, method {Method}",
                payment.Id, rideId, method);

            return new PaymentResultDTO
            {
                IsSuccessful = false,
                TransactionReference = payment.TransactionReference ?? string.Empty,
                PaymentDate = payment.PaymentDate,
                FailureReason = payment.Method == PaymentMethod.cash
                    ? "Awaiting confirmation of cash collection by the driver."
                    : null
            };
        }

        /// <summary>Settles a payment from a verified Stripe webhook event.</summary>
        public async Task ConfirmPaymentAsync(string transactionReference, decimal verifiedAmount, string currency)
        {
            var payment = await _unitOfWork.PaymentRepository
                .Get(p => p.TransactionReference == transactionReference, tracked: true);

            if (payment == null)
            {
                _logger.LogWarning("Webhook referenced an unknown payment: {Reference}", transactionReference);
                return;
            }

            if (payment.Status == PaymentStatus.Completed)
                return;

            // The stored pending payment is authoritative, not the webhook payload.
            if (payment.Amount != verifiedAmount)
            {
                payment.MarkAsFailed("Settled amount did not match the amount due.");
                await _unitOfWork.SaveChangesAsync();
                _logger.LogError("Amount mismatch for payment {PaymentId}: expected {Expected}, settled {Actual}",
                    payment.Id, payment.Amount, verifiedAmount);
                return;
            }

            if (!string.Equals(currency, _currency, StringComparison.OrdinalIgnoreCase))
            {
                payment.MarkAsFailed("Settled currency did not match the expected currency.");
                await _unitOfWork.SaveChangesAsync();
                _logger.LogError("Currency mismatch for payment {PaymentId}: expected {Expected}, got {Actual}",
                    payment.Id, _currency, currency);
                return;
            }

            payment.MarkAsCompleted();

            var ride = await _unitOfWork.RideRepository.GetByIdAsync(payment.RideId);
            ride?.MarkAsPaid();

            await _unitOfWork.RideRepository.UpdateAsync(ride!);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Payment {PaymentId} settled via Stripe webhook; ride {RideId} marked paid",
                payment.Id, payment.RideId);
        }

        /// <summary>Settles a cash payment. Only the assigned driver may confirm.</summary>
        public async Task ConfirmCashCollectionAsync(Guid rideId, Guid driverId)
        {
            var ride = await _unitOfWork.RideRepository.GetByIdAsync(rideId);
            if (ride == null)
                throw new RideNotFoundException("Ride not found.");

            if (ride.DriverId != driverId)
                throw new UnauthorizedAccessException("This ride is not assigned to you.");

            var payment = await _unitOfWork.PaymentRepository
                .Get(p => p.RideId == rideId && p.Status == PaymentStatus.Pending, tracked: true);

            if (payment == null)
                throw new PaymentProcessingException("There is no pending payment for this ride.");

            if (payment.Method != PaymentMethod.cash)
                throw new PaymentProcessingException("This ride is not being paid in cash.");

            payment.MarkAsCompleted();
            ride.MarkAsPaid();

            await _unitOfWork.PaymentRepository.UpdateAsync(payment);
            await _unitOfWork.RideRepository.UpdateAsync(ride);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Cash payment {PaymentId} confirmed by driver {DriverId}; ride {RideId} marked paid",
                payment.Id, driverId, rideId);
        }
    }
}
