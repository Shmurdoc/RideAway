using MediatR;
using RideAway.Domain.Entities;

namespace RideAway.Application.Features.Rides.Queries
{
    public record GetUserByIdQuery(Guid Id) : IRequest<User?>;
}
