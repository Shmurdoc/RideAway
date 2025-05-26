using Moq;

using FluentAssertions;
using RideAway.Domain.Entities;
using RideAway.Application.DTOs;
using RideAway.Domain.Entities.Enum;
using RideAway.Domain.Value_Object;
using RideAway.Domain.Service;
using RideAway.Application.IRepositories;
using RideAway.Application.Features.Rides.Handlers.Commands;
using RideAway.Application.Features.Rides.Commands;
using RideAway.Application.IServices;


namespace RideAway.Tests.Application.Features.Rides.Commands;

public class RequestRideHandlerTests
{

    [Fact]
    public async Task Handle_ShouldReturnRide_WhenValidRequest()
    {
        // Arrange
        var pickupAddress = "1 Copper Street, Phalaborwa, Limpopo, South Africa, 1390";
        var destinationAddress = "12 Mopani Avenue, Phalaborwa, Limpopo, South Africa, 1390";
        var resolvedPickup = new Location(-23.942824, 31.141145, pickupAddress);
        var resolvedDestination = new Location(-23.943670, 31.133789, destinationAddress);
        var calculatedFare = 250.50m;
        var driverId = Guid.NewGuid();

        var dto = new CreateRideRequestDTO
        {
            PickupLocation = pickupAddress,
            Destination = destinationAddress,
            RideCategory = RideCategory.Standard,
            DriverId = driverId
        };

        var command = new RequestRideCommand(dto);

        // Expected ride
        var expectedRide = new Ride(pickupAddress, destinationAddress, calculatedFare)
        {
            DriverId = driverId,
            RiderCategory = dto.RideCategory,
            Status = RideStatus.Requested
        };

        var geoMock = new Mock<IGeoCodingService>();
        geoMock.Setup(x => x.ConvertAddressToLocationAsync(pickupAddress))
               .ReturnsAsync(resolvedPickup);
        geoMock.Setup(x => x.ConvertAddressToLocationAsync(destinationAddress))
               .ReturnsAsync(resolvedDestination);

        var matchingMock = new Mock<IRideMatchingService>();
        matchingMock.Setup(x => x.CalculateFareAsync(resolvedPickup, resolvedDestination, dto.RideCategory))
        .ReturnsAsync(calculatedFare);

        var rideFactoryMock = new Mock<IRideFactory>();
        rideFactoryMock.Setup(x => x.CreateRide(
            pickupAddress,
            destinationAddress,
            calculatedFare,
            driverId))
            .Returns(expectedRide);

        var rideRepoMock = new Mock<IRideRepository>();
        rideRepoMock.Setup(x => x.AddAsync(It.IsAny<Ride>()))
                    .Returns(Task.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.RideRepository).Returns(rideRepoMock.Object);
        unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var handler = new RequestRideHandler(unitOfWorkMock.Object, matchingMock.Object, geoMock.Object, rideFactoryMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Fare.Should().Be(calculatedFare);
        result.PickupLocation.Should().Be(pickupAddress);
        result.Destination.Should().Be(destinationAddress);
        result.DriverId.Should().Be(driverId);
        result.Status.Should().Be(RideStatus.Requested); // ✅ Important

        rideRepoMock.Verify(x => x.AddAsync(It.IsAny<Ride>()), Times.Once);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }



    [Fact]
    public async Task Handle_ShouldThrowException_WhenDriverIsEmpty()
    {
        // Arrange
        var dto = new CreateRideRequestDTO
        {
            PickupLocation = "123 Main St",
            Destination = "456 Elm St",
            RideCategory = RideCategory.Standard,
            DriverId = Guid.Empty
        };

        var command = new RequestRideCommand(dto);

        var handler = new RequestRideHandler(
            Mock.Of<IUnitOfWork>(),
            Mock.Of<IRideMatchingService>(),
            Mock.Of<IGeoCodingService>(),
            Mock.Of<RideFactory>());

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("No available drivers at the moment.");
    }
}
