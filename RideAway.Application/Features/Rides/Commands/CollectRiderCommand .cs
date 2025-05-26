using MediatR;
namespace RideAway.Application.Features.Ride.Commands
{
    public record CollectRiderCommand(Guid RiderId, Guid DriverId ) : IRequest<RideAway.Domain.Entities.Ride>;
}