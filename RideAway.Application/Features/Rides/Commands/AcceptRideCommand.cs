using MediatR;

namespace RideAway.Application.Features.Rides.Commands
{
    public record AcceptRideCommand(Guid RideId, Guid DriverId) : IRequest<bool>;
}
