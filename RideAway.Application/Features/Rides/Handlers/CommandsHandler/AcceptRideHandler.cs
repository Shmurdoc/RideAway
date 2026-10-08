using MediatR;
using Microsoft.Extensions.Logging;
using RideAway.Application.Features.Rides.Commands;
using RideAway.Application.IRepositories;
using RideAway.Domain.Exceptions;

namespace RideAway.Application.Features.Rides.Handlers.Commands
{
    public class AcceptRideHandler : IRequestHandler<AcceptRideCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AcceptRideHandler> _logger;
        public AcceptRideHandler(IUnitOfWork unitOfWork, ILogger<AcceptRideHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> Handle(AcceptRideCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling AcceptRideCommand for RideId: {RideId}, DriverId: {DriverId}", request.RideId, request.DriverId);

            var ride = await _unitOfWork.RideRepository.GetByIdAsync(request.RideId);

            if (ride == null)
            {
                _logger.LogWarning("Ride not found. RideId: {RideId}", request.RideId);
                throw new RideNotFoundException("Ride not found.");
            }

            // Assigns the authenticated driver and enforces the Requested -> Accepted
            // transition inside the aggregate.
            ride.Accept(request.DriverId);

            await _unitOfWork.RideRepository.UpdateAsync(ride);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Ride accepted successfully. RideId: {RideId}, DriverId: {DriverId}", request.RideId, request.DriverId);

            return true;
        }
    }

}
