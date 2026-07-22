using MediatR;
using Microsoft.Extensions.Logging;
using RideAway.Application.DTOs;
using RideAway.Application.Features.Payments.Commands;
using RideAway.Application.IRepositories;
using RideAway.Application.IServices;
using RideAway.Domain.Entities;
using RideAway.Domain.Exceptions;
using RideAway.Domain.Value_Object;

namespace RideAway.Application.Features.Rides.Handlers.Commands
{
    public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, PaymentResultDTO>
    {
        private readonly IPaymentProcessingService _paymentService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProcessPaymentCommandHandler> _logger;

        public ProcessPaymentCommandHandler(IPaymentProcessingService paymentService, IUnitOfWork unitOfWork, ILogger<ProcessPaymentCommandHandler> logger)
        {
            _paymentService = paymentService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<PaymentResultDTO> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing payment for RideId: {RideId}, UserId: {UserId}, Amount: {Amount}", request.RideId, request.UserId, request.Amount);

            var ride = await _unitOfWork.RideRepository.GetByIdAsync(request.RideId);
            if (ride == null)
            {
                _logger.LogWarning("Ride not found for RideId: {RideId}", request.RideId);
                throw new RideNotFoundException("Ride not found.");
            }

            var paymentResult = await _paymentService.ProcessPaymentAsync(request.UserId, request.Amount, request.PaymentMethod);

            if (paymentResult == null)
            {
                _logger.LogError("Payment service returned null for RideId: {RideId}", request.RideId);
                throw new PaymentProcessingException("Payment processing failed unexpectedly.");
            }

            var payment = new Payment
            {
                Amount = request.Amount,
                Method = request.PaymentMethod,
                UserId = request.UserId,
                PaymentDate = paymentResult.PaymentDate,
                TransactionReference = paymentResult.TransactionReference,
                Status = paymentResult.IsSuccessful ? PaymentStatus.Completed : PaymentStatus.Failed
            };

            if (paymentResult.IsSuccessful)
            {
                ride.MarkAsPaid();

                await _unitOfWork.PaymentRepository.AddAsync(payment);
                await _unitOfWork.RideRepository.UpdateAsync(ride);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Payment and ride updates saved successfully for RideId: {RideId}", request.RideId);
            }
            else
            {
                _logger.LogWarning("Payment failed for RideId: {RideId}, UserId: {UserId}", request.RideId, request.UserId);
            }

            return paymentResult;
        }
    }
}
