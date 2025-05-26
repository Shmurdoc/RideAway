using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using RideAway.Application.Features.Rides.Commands;
using RideAway.Application.Features.Rides.Handlers.Commands;
using RideAway.Application.IRepositories;
using RideAway.Domain.Entities;
using RideAway.Domain.Value_Object;
using RideAway.Tests.Moq.Mocks;
using RideAway.Tests.Moq.Factories;

namespace RideAway.Tests.Application.Features.Rides.Commands;

public class CompleteRideHandlerTests
{
    [Fact]
    public async Task Handle_ShouldMarkRideAsCompleted_WhenRideIsInProgress()
    {
        // Arrange
        var mockRideRepository = new Mock<IRideRepository>();
        var ride = new Ride
        {
            RiderId = Guid.NewGuid(),
            DriverId = Guid.NewGuid(),
            Status = RideStatus.InProgress
        };

        mockRideRepository
            .Setup(r => r.GetByIdAsync(ride.Id))
            .ReturnsAsync(ride);

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork
            .Setup(u => u.RideRepository)
            .Returns(mockRideRepository.Object);
        mockUnitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        var mockLogger = new Mock<ILogger<CompleteRideHandler>>();

        var handler = new CompleteRideHandler(mockUnitOfWork.Object, mockLogger.Object);
        var command = new CompleteRideCommand(ride.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        ride.Status.Should().Be(RideStatus.Completed);

        mockRideRepository.Verify(r => r.GetByIdAsync(ride.Id), Times.Once);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Ride marked as completed successfully. RideId: {ride.Id}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }



    [Fact]
    public async Task Handle_ShouldThrowException_WhenRideDoesNotExist()
    {
        // Arrange
        var rideIdThatDoesNotExist = Guid.NewGuid();

        // Mock the RideRepository to return null when queried with any ID
        var rides = RideFactory.GenerateRides(0);
        var rideRepositoryMock = new Mock<IRideRepository>();
         rideRepositoryMock = RideRepositoryMock.GetMockIRideRepository(rides);

        // Mock UnitOfWork to return the mocked RideRepository
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.RideRepository).Returns(rideRepositoryMock.Object);

        // Create handler with mocked dependencies
        var handler = new CompleteRideHandler(unitOfWorkMock.Object, Mock.Of<ILogger<CompleteRideHandler>>());

        var command = new CompleteRideCommand(rideIdThatDoesNotExist);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
                 .ThrowAsync<Exception>()
                 .WithMessage("Ride not found.");
    }



    [Fact]
    public async Task Handle_ShouldThrowException_WhenRideNotInProgress()
    {
        // Arrange
        var ride = new Ride("12 Mopani Avenue, Phalaborwa, Limpopo, South Africa, 1390", "1 Copper Street, Phalaborwa, Limpopo, South Africa, 1390", 30.0m);

        var repoMock = new Mock<IRideRepository>();
        repoMock.Setup(r => r.GetByIdAsync(ride.Id)).ReturnsAsync(ride);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.RideRepository).Returns(repoMock.Object);

        var handler = new CompleteRideHandler(unitOfWork.Object, Mock.Of<ILogger<CompleteRideHandler>>());
        var command = new CompleteRideCommand(ride.Id);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Ride cannot be completed.");
    }
}