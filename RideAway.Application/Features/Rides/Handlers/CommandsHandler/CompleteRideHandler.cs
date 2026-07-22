using MediatR;
using Microsoft.Extensions.Logging;
using RideAway.Application.Features.Rides.Commands;
using RideAway.Application.IRepositories;
using RideAway.Domain.Entities.Enum;
using RideAway.Domain.Value_Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            _logger.LogInformation("Handling CompleteRideCommand for RideId: {RideId}", request.RideId);

            var ride = await _unitOfWork.RideRepository.GetByIdAsync(request.RideId);

            if (ride == null)
            {
                _logger.LogWarning("Ride not found. RideId: {RideId}", request.RideId);
                throw new Exception("Ride not found.");
            }

            if (ride.Status != RideStatus.InProgress)
            {
                _logger.LogWarning("Ride is not in progress. RideId: {RideId}, CurrentStatus: {Status}", request.RideId, ride.Status);
                throw new Exception("Ride cannot be completed.");
            }

            ride.Status = RideStatus.Completed;

            await _unitOfWork.RideRepository.UpdateAsync(ride);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Ride marked as completed successfully. RideId: {RideId}", request.RideId);

            return true;
        }
    }

}
