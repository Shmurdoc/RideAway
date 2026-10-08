using MediatR;

namespace RideAway.Application.Features.Rides.Commands
{
    /// <summary>
    /// <paramref name="RequesterId"/> is the authenticated caller, who must be the
    /// rider or the assigned driver of the ride.
    /// </summary>
    public record CancelRideCommand(Guid RideId, Guid RequesterId) : IRequest<bool>;
}
