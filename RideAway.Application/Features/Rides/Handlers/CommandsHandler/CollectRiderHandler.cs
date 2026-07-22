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
        _logger.LogInformation("Collecting rider for RideId: {RideId}, DriverId: {DriverId}", request.RiderId, request.DriverId);

        var ride = await _unitOfWork.RideRepository.GetByIdAsync(request.RiderId);

        if (ride == null || ride.DriverId != request.DriverId)
        {
            _logger.LogWarning("Invalid ride or driver mismatch. RideId: {RideId}, DriverId: {DriverId}", request.RiderId, request.DriverId);
            throw new RideNotFoundException("Ride not found or driver is not assigned to this ride.");
        }

        if (ride.Status != RideStatus.Accepted)
        {
            _logger.LogWarning("Invalid ride status for collection. RideId: {RideId}, CurrentStatus: {Status}", request.RiderId, ride.Status);
            throw new InvalidRideStatusException("Ride must be in 'Accepted' state before collecting the rider.");
        }

        if (ride.Driver == null || string.IsNullOrEmpty(ride.Driver.CurrentLocation))
        {
            _logger.LogError("Driver details missing for RideId: {RideId}", request.RiderId);
            throw new InvalidOperationException("Driver information is incomplete for this ride.");
        }

        var currentLocation = await _geocodingService.ConvertAddressToLocationAsync(ride.Driver.CurrentLocation);
        var pickupLocation = await _geocodingService.ConvertAddressToLocationAsync(ride.PickupLocation);

        var route = await _googleMapsApi.GetRouteAsync(currentLocation, pickupLocation);
        _logger.LogInformation("Driver navigation route: {Route}", route);

        ride.Status = RideStatus.InProgress;
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Rider collected successfully. RideId: {RideId}", request.RiderId);

        return ride;
    }
}
}
