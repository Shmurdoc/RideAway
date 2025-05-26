using AutoMapper;
using MediatR;
using RideAway.Application.DTOs;
using RideAway.Application.Features.Rides.Queries;
using RideAway.Application.IServices;

namespace RideAway.Application.Features.Rides.Handlers.Queries
{
    public class GetAvailableRidesHandler : IRequestHandler<GetAvailableRidesQuery, List<RideDTO>>
    {

        private readonly IRideMatchingService _rideMatchingService;

        public GetAvailableRidesHandler(IMapper mapper, IRideMatchingService rideMatchingService, ILocationService locationService, IGeoCodingService geocodingService)
        {
            _rideMatchingService = rideMatchingService;
        }

        public async Task<List<RideDTO>> Handle(GetAvailableRidesQuery request, CancellationToken cancellationToken)
        {
            var rides = await _rideMatchingService.FindDriverAsync(request.PickupLocation, request.Destination, request.RideCategory);

            // Always return a non-null list
            return rides ?? new List<RideDTO>();
        }
    }
}
