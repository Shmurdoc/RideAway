using MediatR;
using RideAway.Application.DTOs;

namespace RideAway.Application.Features.Rides.Queries
{
    /// <summary>
    /// Returns a user profile. <paramref name="RequesterId"/> is the authenticated
    /// caller; the handler refuses to return data for anyone else unless the caller
    /// is an administrator, so this cannot be used to enumerate users.
    /// </summary>
    public record GetUserByIdQuery(Guid Id, Guid RequesterId, bool RequesterIsAdmin = false) : IRequest<UserProfileDTO?>;
}
