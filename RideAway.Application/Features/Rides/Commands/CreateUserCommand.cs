using MediatR;
using RideAway.Application.DTOs;

namespace RideAway.Application.Features.Rides.Commands
{
    public record CreateUserCommand(CreateUserDTO createUserDTO) : IRequest<UserProfileDTO>;
}
