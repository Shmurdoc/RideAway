using MediatR;
using RideAway.Application.Features.Rides.Commands;
using RideAway.Application.IRepositories;
using RideAway.Domain.Exceptions;

namespace RideAway.Application.Features.Rides.Handlers.Commands
{
    public class UpdateDriverLocationHandler : IRequestHandler<UpdateDriverLocationCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateDriverLocationHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateDriverLocationCommand request, CancellationToken cancellationToken)
        {
            // The driver being updated is always the authenticated caller, so a driver
            // can only move their own location.
            var driver = await _unitOfWork.UserRepository.GetByIdAsync(request.DriverId);

            if (driver == null)
                throw new KeyNotFoundException("Driver not found.");

            if (driver.Role != RideAway.Domain.Entities.Enum.UserRole.Driver)
                throw new UnauthorizedAccessException("Only drivers can report a location.");

            if (string.IsNullOrWhiteSpace(request.driverLocationUpdateDTO.CurrentLocation))
                throw new ArgumentException("A location is required.");

            driver.CurrentLocation = request.driverLocationUpdateDTO.CurrentLocation;

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
