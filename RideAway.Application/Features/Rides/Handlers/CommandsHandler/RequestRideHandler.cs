using MediatR;
using RideAway.Application.Features.Rides.Commands;
using RideAway.Application.IRepositories;
using RideAway.Application.IServices;
using RideAway.Domain.Service;
using RideAlias = RideAway.Domain.Entities.Ride;

namespace RideAway.Application.Features.Rides.Handlers.Commands
{
    public class RequestRideHandler : IRequestHandler<RequestRideCommand, RideAlias>
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

        public async Task<RideAlias> Handle(RequestRideCommand request, CancellationToken cancellationToken)
        {
            var nearestDriver = request.CreateRideRequestDTO.DriverId;

            if (nearestDriver == Guid.Empty)
            {
                throw new Exception("No available drivers at the moment.");
            }

            // If Pickup & Destination is a String Address Convert to Location 
            var pickupLocation = await _geocodingService.ConvertAddressToLocationAsync(request.CreateRideRequestDTO.PickupLocation);
            var destination = await _geocodingService.ConvertAddressToLocationAsync(request.CreateRideRequestDTO.Destination);
            //var pickupLocation = request.CreateRideRequestDTO.PickupLocation;
            //var destination = request.CreateRideRequestDTO.Destination;

            // Calculate fare of ride based on ride category
            var fare = await _rideMatchingService.CalculateFareAsync(
                pickupLocation,
                destination, request.CreateRideRequestDTO.RideCategory
            );

            var ride = _rideFactory.CreateRide(
            request.CreateRideRequestDTO.PickupLocation,
            request.CreateRideRequestDTO.Destination,
            fare,
            nearestDriver
            );


            await _unitOfWork.RideRepository.AddAsync(ride);
            await _unitOfWork.SaveChangesAsync();

            return ride;
        }
    }
}
