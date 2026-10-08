using MediatR;

namespace RideAway.Application.Features.Rides.Commands
{
    /// <summary>DriverId is the caller from the token.</summary>
    public record CompleteRideCommand(Guid RideId, Guid DriverId) : IRequest<bool>;
}
