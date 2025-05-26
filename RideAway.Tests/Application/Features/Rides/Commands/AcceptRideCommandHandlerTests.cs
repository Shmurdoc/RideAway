using FluentAssertions;
using Moq;
using RideAway.Application.Features.Rides.Commands;
using RideAway.Domain.Entities;
using Microsoft.Extensions.Logging;
using RideAway.Tests.Moq;
using RideAway.Domain.Value_Object;
using RideAway.Application.Features.Rides.Handlers.Commands;
using RideAway.Tests.Moq.Mocks;
using RideAway.Tests.Moq.Factories;
using RideAway.Application.IRepositories;

namespace RideAway.Tests.Application.Features.Rides.Commands
{
    public class AcceptRideCommandHandlerTests
    {
        private readonly Mock<ILogger<AcceptRideHandler>> _loggerMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly AcceptRideHandler _handler;

        public AcceptRideCommandHandlerTests()
        {
            _handler = new AcceptRideHandler(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldAcceptRide_WhenRideExistsAndIsRequested()
        {
            // Arrange
            var ride = RideFactory.GenerateSingleRide(Guid.NewGuid(), Guid.Empty);
            ride.Status = RideStatus.Requested;

            var rides = new List<Ride> { ride };
            var rideRepoMock = RideRepositoryMock.GetMockIRideRepository(rides);

        
            _unitOfWorkMock.Setup(u => u.RideRepository).Returns(rideRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var handler = new AcceptRideHandler(_unitOfWorkMock.Object, _loggerMock.Object);
            var command = new AcceptRideCommand(ride.Id, Guid.NewGuid());

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            rideRepoMock.Verify(r => r.UpdateAsync(It.Is<Ride>(x =>
                x.Id == ride.Id && x.Status == RideStatus.Accepted && x.DriverId == command.DriverId)), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenRideNotFound()
        {
            // Arrange
            var rides = new List<Ride>(); // Empty list simulates not found
            var rideRepoMock = RideRepositoryMock.GetMockIRideRepository(rides);

            _unitOfWorkMock.Setup(u => u.RideRepository).Returns(rideRepoMock.Object);

            var notFoundId = Guid.NewGuid();
            var handler = new AcceptRideHandler(_unitOfWorkMock.Object, _loggerMock.Object);
            var command = new AcceptRideCommand(notFoundId, Guid.NewGuid());

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Ride not found.");

            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenRideIsNotInRequestedStatus()
        {
            // Arrange
            var ride = RideFactory.GenerateSingleRide(Guid.NewGuid(), Guid.Empty);
            ride.Status = RideStatus.Accepted;

            var rides = new List<Ride> { ride };
            var rideRepoMock = RideRepositoryMock.GetMockIRideRepository(rides);

            
            _unitOfWorkMock.Setup(u => u.RideRepository).Returns(rideRepoMock.Object);

            var handler = new AcceptRideHandler(_unitOfWorkMock.Object, _loggerMock.Object);
            var command = new AcceptRideCommand(ride.Id, Guid.NewGuid());

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Ride has already been accepted or is not available.");

            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldSetDriverId_WhenAcceptingRide()
        {
            // Arrange
            var ride = RideFactory.GenerateSingleRide(Guid.NewGuid(), Guid.Empty);
            ride.Status = RideStatus.Requested;
            var driverId = Guid.NewGuid();

            var rides = new List<Ride> { ride };
            var rideRepoMock = RideRepositoryMock.GetMockIRideRepository(rides);

            
            _unitOfWorkMock.Setup(u => u.RideRepository).Returns(rideRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var handler = new AcceptRideHandler(_unitOfWorkMock.Object, _loggerMock.Object);
            var command = new AcceptRideCommand(ride.Id, driverId);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            ride.DriverId.Should().Be(driverId);
            ride.Status.Should().Be(RideStatus.Accepted);
        }


    }
}
