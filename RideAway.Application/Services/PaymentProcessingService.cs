using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RideAway.Application.DTOs;
using RideAway.Application.IRepositories;
using RideAway.Application.IServices;
using RideAway.Domain.Entities;
using RideAway.Domain.Entities.Enum;
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
            _currency = configuration["StripeSettings:Currency"] ?? "ZAR";
        }

        public async Task<bool> ProcessPayment(Guid paymentId)
        {
            var payment = await _unitOfWork.PaymentRepository.GetByIdAsync(paymentId);

            if (payment == null)
            {
                _logger.LogWarning("Payment not found for ID: {PaymentId}", paymentId);
                throw new Exception("Payment not found.");
            }

            if (payment.Status == PaymentStatus.Completed)
            {
                _logger.LogInformation("Payment already completed for ID: {PaymentId}", paymentId);
                return true; // Already processed
            }

            payment.Status = PaymentStatus.Completed;
            payment.IsSuccessful = true;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Payment marked as completed successfully. ID: {PaymentId}", paymentId);

            return true;
        }

        public async Task<PaymentResultDTO> ProcessPaymentAsync(Guid userId, decimal amount, PaymentMethod method)
        {
            var payment = new Payment
            {
                UserId = userId,
                Amount = amount,
                Method = method,
                PaymentDate = DateTime.UtcNow,
                Status = PaymentStatus.Pending,
                IsSuccessful = false
            };

            bool success = false;

            switch (method)
            {
                case PaymentMethod.cash:
                    success = true;
                    break;

                case PaymentMethod.card:
                case PaymentMethod.stripe:
                    if (_stripeService == null)
                        throw new InvalidOperationException("Stripe service is not configured.");

                    var result = await _stripeService.CreatePaymentSession(amount, _currency);
                    payment.TransactionReference = result.Reference;
                    success = result.Success;
                    break;

                default:
                    throw new NotImplementedException($"Payment method {method} not supported.");
            }

            payment.IsSuccessful = success;
            payment.Status = success ? PaymentStatus.Completed : PaymentStatus.Failed;

            await _unitOfWork.PaymentRepository.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Payment processed for user {UserId}. Method: {Method}, Success: {Success}", userId, method, success);

            return new PaymentResultDTO
            {
                IsSuccessful = payment.IsSuccessful,
                TransactionReference = payment.TransactionReference!,
                PaymentDate = payment.PaymentDate
            };
        }
    }
}
