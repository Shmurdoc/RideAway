using MediatR;
using RideAway.Application.DTOs;
using RideAway.Application.Features.Rides.Queries;
using RideAway.Application.IRepositories;

namespace RideAway.Application.Features.Rides.Handlers.Queries
{
    public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserProfileDTO?>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUserByIdHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<UserProfileDTO?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            if (request.Id != request.RequesterId && !request.RequesterIsAdmin)
                throw new UnauthorizedAccessException("You are not authorized to view this user.");

            var user = await _unitOfWork.UserRepository.GetByIdAsync(request.Id);
            if (user is null)
                return null;

            return new UserProfileDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role
            };
        }
    }
}
