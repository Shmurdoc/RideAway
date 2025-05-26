using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using RideAway.Application.IRepositories;
using RideAway.Application.IServices;
using RideAway.Domain.Entities;
using RideAway.Domain.Value_Object;
using RideAway.Tests.Moq.Factories;
using RideAway.Application.Features.Ride.Commands;

namespace RideAway.Tests.Application.Features.Rides.Commands;

public class CollectRiderHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateRideStatus_WhenValidRequestIsProvided()
    {
        // Arrange
        var  rideId = Guid.NewGuid();
        var driverId = Guid.NewGuid();

        var ride = RideFactory.GenerateSingleRide(rideId, driverId);
        ride.Status = RideStatus.Accepted; // Ensure it starts in a valid state

        var mockRideRepo = new Mock<IRideRepository>();
        mockRideRepo.Setup(r => r.GetByIdAsync(ride.Id)).ReturnsAsync(ride);

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(u => u.RideRepository).Returns(mockRideRepo.Object);
        mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var mockGeoCoding = new Mock<IGeoCodingService>();
        mockGeoCoding.Setup(g => g.ConvertAddressToLocationAsync(It.IsAny<string>()))
                     .ReturnsAsync(new Location(-23.942824, 31.141145, "1 Copper Street, Phalaborwa, Limpopo, South Africa, 1390"));

        var mockMapsApi = new Mock<IGoogleMapsApi>();
        mockMapsApi.Setup(g => g.GetRouteAsync(It.IsAny<Location>(), It.IsAny<Location>()))
                   .ReturnsAsync("20 Copper Street, Phalaborwa, Limpopo, South Africa, 1390");

        var mockLogger = new Mock<ILogger<CollectRiderHandler>>();

        var handler = new CollectRiderHandler(
            mockUnitOfWork.Object,
            mockMapsApi.Object,
            mockGeoCoding.Object,
            mockLogger.Object
        );

        var command = new CollectRiderCommand(ride.Id, driverId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(RideStatus.InProgress);
        result.Id.Should().Be(ride.Id);

        mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }



    [Fact]
    public async Task Handle_ShouldThrowException_WhenRideNotFound()
    {
        // Arrange
        var rideId = Guid.NewGuid();
        var driverId = Guid.NewGuid();

        var mockRideRepo = new Mock<IRideRepository>();
        mockRideRepo.Setup(r => r.GetByIdAsync(rideId)).ReturnsAsync((Ride)null);

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(u => u.RideRepository).Returns(mockRideRepo.Object);

        var handler = new CollectRiderHandler(
            mockUnitOfWork.Object,
            Mock.Of<IGoogleMapsApi>(),
            Mock.Of<IGeoCodingService>(),
            Mock.Of<ILogger<CollectRiderHandler>>()
        );

        var command = new CollectRiderCommand(rideId, driverId);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
                 .WithMessage("Ride not found or driver is not assigned to this ride.");
    }
}
