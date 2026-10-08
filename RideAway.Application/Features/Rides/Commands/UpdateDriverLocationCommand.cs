using MediatR;
using RideAway.Application.DTOs;

namespace RideAway.Application.Features.Rides.Commands
{
    /// <summary>
    /// A driver may only update their own location; <paramref name="DriverId"/> is the
    /// authenticated caller.
    /// </summary>
    public record UpdateDriverLocationCommand(DriverLocationUpdateDTO driverLocationUpdateDTO, Guid DriverId) : IRequest<bool>;
}
