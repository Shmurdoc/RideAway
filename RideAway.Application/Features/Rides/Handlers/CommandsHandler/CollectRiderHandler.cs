using MediatR;
using Microsoft.Extensions.Logging;
using RideAway.Application.Features.Rides.Commands;
using RideAway.Application.IRepositories;
using RideAway.Application.IServices;
using RideAway.Domain.Entities;
using RideAway.Domain.Value_Object;
using RideAway.Domain.Exceptions;

namespace RideAway.Application.Features.Rides.Handlers.Commands
{
    public class CollectRiderHandler : IRequestHandler<CollectRiderCommand, Ride>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGoogleMapsApi _googleMapsApi;
        private readonly IGeoCodingService _geocodingService;
        private readonly ILogger<CollectRiderHandler> _logger;

        public CollectRiderHandler(
            IUnitOfWork unitOfWork,
            IGoogleMapsApi googleMapsApi,
            IGeoCodingService geocodingService,
            ILogger<CollectRiderHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _googleMapsApi = googleMapsApi;
            _geocodingService = geocodingService;
            _logger = logger;
        }

        public async Task<Ride> Handle(CollectRiderCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Collecting rider for RideId: {RideId}, DriverId: {DriverId}", request.RideId, request.DriverId);

            var ride = await _unitOfWork.RideRepository.GetByIdAsync(request.RideId);

            if (ride == null)
            {
                _logger.LogWarning("Ride not found. RideId: {RideId}", request.RideId);
                throw new RideNotFoundException("Ride not found.");
            }

            ride.StartTrip(request.DriverId);

            if (ride.Driver == null || string.IsNullOrEmpty(ride.Driver.CurrentLocation))
            {
                _logger.LogError("Driver details missing for RideId: {RideId}", request.RideId);
                throw new InvalidOperationException("Driver information is incomplete for this ride.");
            }

            var currentLocation = await _geocodingService.ConvertAddressToLocationAsync(ride.Driver.CurrentLocation);
            var pickupLocation = await _geocodingService.ConvertAddressToLocationAsync(ride.PickupLocation);

            if (currentLocation is null || pickupLocation is null)
            {
                _logger.LogError("Could not geocode driver or pickup location for RideId: {RideId}", request.RideId);
                throw new InvalidOperationException("Driver or pickup location could not be resolved for this ride.");
            }

            var route = await _googleMapsApi.GetRouteAsync(currentLocation, pickupLocation);
            _logger.LogDebug("Driver navigation route computed for ride {RideId}", request.RideId);

            await _unitOfWork.RideRepository.UpdateAsync(ride);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Rider collected successfully. RideId: {RideId}", request.RideId);

            return ride;
        }
    }
}
