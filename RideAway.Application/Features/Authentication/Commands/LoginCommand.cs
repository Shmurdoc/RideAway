using MediatR;
using RideAway.Domain.Entities;

namespace RideAway.Application.Features.Authentication.Commands
{
    public record LoginCommand(string Email, string Password) : IRequest<User>;
}
