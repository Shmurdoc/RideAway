using MediatR;
using RideAway.Application.DTOs;
using RideAway.Domain.Entities;

namespace RideAway.Application.Features.Rides.Commands
{
    public record CreateUserCommand(CreateUserDTO createUserDTO) : IRequest<User>;
}
