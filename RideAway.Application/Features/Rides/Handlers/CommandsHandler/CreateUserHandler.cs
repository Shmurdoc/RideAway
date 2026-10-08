using MediatR;
using Microsoft.Extensions.Logging;
using RideAway.Application.DTOs;
using RideAway.Application.Features.Rides.Commands;
using RideAway.Application.IRepositories;
using RideAway.Application.Services;
using RideAway.Domain.Entities;
using RideAway.Domain.Entities.Enum;

namespace RideAway.Application.Features.Rides.Handlers.Commands
{
    public class CreateUserHandler : IRequestHandler<CreateUserCommand, UserProfileDTO>
    {
        private const int MinimumPasswordLength = 12;
        private const int MaximumPasswordLength = 128;

        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateUserHandler> _logger;

        public CreateUserHandler(IUnitOfWork unitOfWork, ILogger<CreateUserHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<UserProfileDTO> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var dto = request.createUserDTO;

            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new ArgumentException("Email is required.");

            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new ArgumentException("Password is required.");

            if (dto.Password.Length < MinimumPasswordLength || dto.Password.Length > MaximumPasswordLength)
                throw new ArgumentException($"Password must be between {MinimumPasswordLength} and {MaximumPasswordLength} characters.");

            var email = dto.Email.Trim().ToLowerInvariant();

            var existingUser = await _unitOfWork.UserRepository.Get(u => u.Email == email);
            if (existingUser != null)
                throw new ArgumentException("A user with this email already exists.");

            // Admin is not self-assignable; only Driver is honoured.
            var role = dto.Role == UserRole.Driver ? UserRole.Driver : UserRole.Rider;

            var user = new User
            {
                Name = dto.Name?.Trim() ?? string.Empty,
                Email = email,
                PhoneNumber = dto.PhoneNumber,
                Role = role,
                PasswordHash = PasswordHasher.Hash(dto.Password)
            };

            await _unitOfWork.UserRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("New user created with ID: {UserId} and Role: {Role}", user.Id, user.Role);

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
