using MediatR;
using Microsoft.Extensions.Logging;
using RideAway.Application.DTOs;
using RideAway.Application.Features.Payments.Commands;
using RideAway.Application.IRepositories;
using RideAway.Application.IServices;
using RideAway.Domain.Entities;
using RideAway.Domain.Entities.Enum;
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
            var ride = await _unitOfWork.RideRepository.GetByIdAsync(request.RideId);
            if (ride == null)
                throw new RideNotFoundException("Ride not found.");

            // Only the rider who requested the ride may pay for it.
            if (ride.RiderId != request.RiderId)
                throw new UnauthorizedAccessException("You are not authorized to pay for this ride.");

            // A settled ride is terminal: replaying the request must not move money again.
            if (ride.Status == RideStatus.Paid)
                throw new PaymentProcessingException("This ride has already been paid.");

            if (ride.Status != RideStatus.Completed)
                throw new InvalidRideStatusException($"A ride cannot be paid while it is {ride.Status}.");

            // The amount comes from the server-computed fare, never from the request.
            var amount = ride.Fare;
            if (amount <= 0)
                throw new PaymentProcessingException("This ride has no payable fare.");

            _logger.LogInformation("Starting payment for RideId: {RideId}, RiderId: {RiderId}, Amount: {Amount}, Method: {Method}",
                request.RideId, request.RiderId, amount, request.PaymentMethod);

            var paymentResult = await _paymentService.CreatePaymentAsync(
                ride.Id, request.RiderId, amount, request.PaymentMethod);

            if (paymentResult == null)
            {
                _logger.LogError("Payment service returned null for RideId: {RideId}", request.RideId);
                throw new PaymentProcessingException("Payment processing failed unexpectedly.");
            }

            // The ride is not marked paid here. It is marked paid only when a trusted
            // source settles the payment: a signature-verified Stripe webhook for card
            // payments, or the assigned driver confirming cash collection.
            return paymentResult;
        }
    }
}
