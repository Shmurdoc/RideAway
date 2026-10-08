using MediatR;

namespace RideAway.Application.Features.Rides.Commands
{
    /// <summary>DriverId is the caller from the token.</summary>
    public record CollectRiderCommand(Guid RideId, Guid DriverId) : IRequest<RideAway.Domain.Entities.Ride>;
}
