using MediatR;
using RideAway.Application.DTOs;
using RideAway.Domain.Entities;

namespace RideAway.Application.Features.Rides.Commands
{
    public record RequestRideCommand(CreateRideRequestDTO CreateRideRequestDTO) : IRequest<RideAway.Domain.Entities.Ride>;
}
