using MediatR;
using Microsoft.Extensions.Logging;
using RideAway.Application.Features.Rides.Commands;
using RideAway.Application.IRepositories;
using RideAway.Domain.Exceptions;

namespace RideAway.Application.Features.Rides.Handlers.Commands
{
    public class CompleteRideHandler : IRequestHandler<CompleteRideCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CompleteRideHandler> _logger;
        public CompleteRideHandler(IUnitOfWork unitOfWork, ILogger<CompleteRideHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> Handle(CompleteRideCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling CompleteRideCommand for RideId: {RideId}, DriverId: {DriverId}", request.RideId, request.DriverId);

            var ride = await _unitOfWork.RideRepository.GetByIdAsync(request.RideId);

            if (ride == null)
            {
                _logger.LogWarning("Ride not found. RideId: {RideId}", request.RideId);
                throw new RideNotFoundException("Ride not found.");
            }

            // Only the assigned driver may complete, and only from InProgress.
            ride.Complete(request.DriverId);

            await _unitOfWork.RideRepository.UpdateAsync(ride);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Ride marked as completed successfully. RideId: {RideId}", request.RideId);

            return true;
        }
    }

}
