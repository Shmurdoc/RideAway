using MediatR;
using RideAway.Application.DTOs;
using RideAway.Domain.Value_Object;

namespace RideAway.Application.Features.Rides.Commands
{
    public record UpdateDriverLocationCommand(DriverLocationUpdateDTO driverLocationUpdateDTO) : IRequest<bool>;
}
