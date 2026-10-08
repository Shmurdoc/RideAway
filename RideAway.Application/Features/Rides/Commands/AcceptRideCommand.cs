using MediatR;

namespace RideAway.Application.Features.Rides.Commands
{
    /// <summary>
    /// The driver is the authenticated caller, not a client-supplied value.
    /// </summary>
    public record AcceptRideCommand(Guid RideId, Guid DriverId) : IRequest<bool>;
}
