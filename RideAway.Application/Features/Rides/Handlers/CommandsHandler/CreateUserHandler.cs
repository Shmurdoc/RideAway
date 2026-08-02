using MediatR;
using Microsoft.Extensions.Logging;
using RideAway.Application.Features.Rides.Commands;
using RideAway.Application.IRepositories;
using RideAway.Application.Services;
using RideAway.Domain.Entities;

namespace RideAway.Application.Features.Rides.Handlers.Commands
{
    public class CreateUserHandler : IRequestHandler<CreateUserCommand, User>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateUserHandler> _logger;

        public CreateUserHandler(IUnitOfWork unitOfWork, ILogger<CreateUserHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<User> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.createUserDTO.Email))
                throw new ArgumentException("Email is required.");

            var existingUser = await _unitOfWork.UserRepository.Get(u => u.Email == request.createUserDTO.Email);
            if (existingUser != null)
                throw new ArgumentException("A user with this email already exists.");

            var user = new User
            {
                Name = request.createUserDTO.Name,
                Email = request.createUserDTO.Email,
                PhoneNumber = request.createUserDTO.PhoneNumber,
                Role = request.createUserDTO.Role,
                PasswordHash = PasswordHasher.Hash(request.createUserDTO.Password)
            };

            await _unitOfWork.UserRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("New user created with ID: {UserId} and Role: {Role}", user.Id, user.Role);

            return user;
        }
    }
}
