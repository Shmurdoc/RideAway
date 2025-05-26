using MediatR;
using Microsoft.Extensions.Logging;
using RideAway.Application.Features.Rides.Commands;
using RideAway.Application.IRepositories;
using RideAway.Domain.Entities.Enum;
using RideAway.Domain.Value_Object;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RideAway.Application.Features.Rides.Handlers.Commands
{
    public class CancelRideHandler : IRequestHandler<CancelRideCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CancelRideHandler> _logger;

        public CancelRideHandler(IUnitOfWork unitOfWork, ILogger<CancelRideHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> Handle(CancelRideCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling CancelRideCommand for RideId: {RideId}", request.RideId);

            var ride = await _unitOfWork.RideRepository.GetByIdAsync(request.RideId);

            if (ride == null)
            {
                _logger.LogWarning("Ride not found. RideId: {RideId}", request.RideId);
                throw new Exception("Ride cannot be canceled.");
            }

            if (ride.Status == RideStatus.Completed)
            {
                _logger.LogWarning("Cannot cancel a completed ride. RideId: {RideId}", request.RideId);
                throw new Exception("Ride cannot be canceled.");
            }

            ride.Status = RideStatus.Canceled;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Ride canceled successfully. RideId: {RideId}", request.RideId);

            return true;
        }
    }
}
