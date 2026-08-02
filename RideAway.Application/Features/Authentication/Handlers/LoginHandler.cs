using MediatR;
using RideAway.Application.Features.Authentication.Commands;
using RideAway.Application.IRepositories;
using RideAway.Application.Services;
using RideAway.Domain.Entities;
using RideAway.Domain.Exceptions;

namespace RideAway.Application.Features.Authentication.Handlers
{
    public class LoginHandler : IRequestHandler<LoginCommand, User>
    {
        private readonly IUnitOfWork _unitOfWork;

        public LoginHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<User> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.UserRepository.Get(
                u => u.Email == request.Email,
                tracked: true);

            if (user == null || string.IsNullOrEmpty(user.PasswordHash) ||
                !PasswordHasher.Verify(request.Password, user.PasswordHash))
            {
                throw new InvalidCredentialsException("Invalid email or password.");
            }

            return user;
        }
    }
}
