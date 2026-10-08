using MediatR;
using RideAway.Application.DTOs;

namespace RideAway.Application.Features.Rides.Commands
{
    /// <summary>DriverId is the caller from the token.</summary>
    public record UpdateDriverLocationCommand(DriverLocationUpdateDTO driverLocationUpdateDTO, Guid DriverId) : IRequest<bool>;
}
