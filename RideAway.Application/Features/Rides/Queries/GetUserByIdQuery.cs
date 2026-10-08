using MediatR;
using RideAway.Application.DTOs;

namespace RideAway.Application.Features.Rides.Queries
{
    /// <summary>RequesterId is the caller from the token. Cross-user reads need admin.</summary>
    public record GetUserByIdQuery(Guid Id, Guid RequesterId, bool RequesterIsAdmin = false) : IRequest<UserProfileDTO?>;
}
