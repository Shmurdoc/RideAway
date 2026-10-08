using MediatR;

namespace RideAway.Application.Features.Rides.Commands
{
    /// <summary>RequesterId is the caller from the token.</summary>
    public record CancelRideCommand(Guid RideId, Guid RequesterId) : IRequest<bool>;
}
