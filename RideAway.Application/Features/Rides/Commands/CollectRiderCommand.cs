using MediatR;

namespace RideAway.Application.Features.Rides.Commands
{
    /// <summary>
    /// The driver is the authenticated caller, not a client-supplied value.
    /// </summary>
    public record CollectRiderCommand(Guid RideId, Guid DriverId) : IRequest<RideAway.Domain.Entities.Ride>;
}
