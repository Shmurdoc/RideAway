using MediatR;

namespace RideAway.Application.Features.Rides.Queries
{
    public record GetRideByIdQuery(Guid RideId) : IRequest<RideAway.Domain.Entities.Ride>;

}
