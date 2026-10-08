using MediatR;
using RideAway.Application.Features.Rides.Commands;
using RideAway.Application.IRepositories;
using RideAway.Application.IServices;
using RideAway.Domain.Exceptions;
using RideAway.Domain.Service;

namespace RideAway.Application.Features.Rides.Handlers.Commands
{
    public class RequestRideHandler : IRequestHandler<RequestRideCommand, RideAway.Domain.Entities.Ride>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGeoCodingService _geocodingService;
        private readonly IRideMatchingService _rideMatchingService;
        private readonly IRideFactory _rideFactory;

        public RequestRideHandler(IUnitOfWork unitOfWork, IRideMatchingService rideMatchingService, IGeoCodingService geocodingService, IRideFactory rideFactory)
        {
            _geocodingService = geocodingService;
            _unitOfWork = unitOfWork;
            _rideMatchingService = rideMatchingService;
            _rideFactory = rideFactory;
        }

        public async Task<RideAway.Domain.Entities.Ride> Handle(RequestRideCommand request, CancellationToken cancellationToken)
        {
            var dto = request.CreateRideRequestDTO;

            if (string.IsNullOrWhiteSpace(dto.PickupLocation))
                throw new ArgumentException("A pickup location is required.");

            if (string.IsNullOrWhiteSpace(dto.Destination))
                throw new ArgumentException("A destination is required.");

            if (request.RiderId == Guid.Empty)
                throw new UnauthorizedAccessException("The authenticated token does not identify a rider.");

            var pickupLocation = await _geocodingService.ConvertAddressToLocationAsync(dto.PickupLocation);
            var destination = await _geocodingService.ConvertAddressToLocationAsync(dto.Destination);

            if (pickupLocation is null || destination is null)
            {
                throw new InvalidOperationException("Could not geocode the pickup or destination address.");
            }

            var fare = await _rideMatchingService.CalculateFareAsync(
                pickupLocation,
                destination,
                dto.RideCategory
            );

            // The driver is chosen by the matching service, not supplied by the client,
            // and the rider is always the authenticated caller.
            Guid? driverId = dto.DriverId == Guid.Empty ? null : dto.DriverId;

            var ride = _rideFactory.CreateRide(
                dto.PickupLocation,
                dto.Destination,
                fare,
                request.RiderId,
                driverId
            );

            await _unitOfWork.RideRepository.AddAsync(ride);
            await _unitOfWork.SaveChangesAsync();

            return ride;
        }
    }
}
