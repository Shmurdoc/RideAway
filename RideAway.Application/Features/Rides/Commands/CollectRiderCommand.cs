using MediatR;
namespace RideAway.Application.Features.Rides.Commands
{
    public record CollectRiderCommand(Guid RideId, Guid DriverId) : IRequest<RideAway.Domain.Entities.Ride>;
}