using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RideAway.Application.Features.Rides.Commands;
using RideAway.Application.Features.Rides.Handlers.Commands;
using RideAway.Application.IRepositories;
using RideAway.Domain.Entities;
using RideAway.Domain.Service;
using RideAway.Domain.Value_Object;
using RideAlias = RideAway.Domain.Entities.Ride;

namespace RideAway.Tests.Application.Features.Rides.Commands
{
    public class CancelRideHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<CancelRideHandler>> _mockLogger;
        private readonly Mock<IRideRepository> _mockRideRepository;
        private readonly string pickup = "Pickup";
        private readonly string destination = "Destination";

        public CancelRideHandlerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<CancelRideHandler>>();
            _mockRideRepository = new Mock<IRideRepository>();
            _mockUnitOfWork.Setup(x => x.RideRepository).Returns(_mockRideRepository.Object);
        }

        [Fact]
        public async Task Handle_ShouldCancelRide_WhenRideIsValid()
        {
            // Arrange
            var ride = new RideAlias(pickup: ("123, Start Point"),
                                     destination: ("456, End Point"),
                                     fare: 100);
            ride.Status = RideStatus.Accepted; // Ensure the ride is active and valid for cancellation

            var command = new CancelRideCommand(ride.Id);

            _mockRideRepository.Setup(repo => repo.GetByIdAsync(ride.Id))
                               .ReturnsAsync(ride);

            var handler = new CancelRideHandler(_mockUnitOfWork.Object, _mockLogger.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            ride.Status.Should().Be(RideStatus.Canceled); // Verify the ride status is updated
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once); // Ensure changes are persisted

            // Verify logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Ride canceled successfully")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }


        [Fact]
        public async Task Handle_ShouldThrowException_WhenRideNotFound()
        {
            // Arrange

            var rideId = Guid.NewGuid();
            var command = new CancelRideCommand(rideId);

            _mockRideRepository.Setup(repo => repo.GetByIdAsync(rideId))
                               .ReturnsAsync((RideAlias?)null);

            var handler = new CancelRideHandler(_mockUnitOfWork.Object, _mockLogger.Object);

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Ride cannot be canceled.");

            // Verify warning log
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Ride not found")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenRideIsCompleted()
        {
            // Arrange
            var pickup = ("123, Start Point");
            var destination = ("456, End Point");
            var ride = new RideAlias(pickup: pickup, destination: destination, fare: 50)
            {
                Status = RideStatus.Completed
            };

            var command = new CancelRideCommand(ride.Id);

            _mockRideRepository.Setup(repo => repo.GetByIdAsync(ride.Id))
                               .ReturnsAsync(ride);

            var handler = new CancelRideHandler(_mockUnitOfWork.Object, _mockLogger.Object);

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Ride cannot be canceled.");

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cannot cancel a completed ride")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
}
}
