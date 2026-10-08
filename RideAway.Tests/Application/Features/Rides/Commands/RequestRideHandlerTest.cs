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

        var riderId = Guid.NewGuid();
        var command = new RequestRideCommand(dto, riderId);

        var expectedRide = new Ride(pickupAddress, destinationAddress, calculatedFare)
        {
            DriverId = driverId,
            RiderId = riderId,
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
            riderId,
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
        result.Status.Should().Be(RideStatus.Requested);

        rideRepoMock.Verify(x => x.AddAsync(It.IsAny<Ride>()), Times.Once);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRecordTheAuthenticatedRider_SoTheRideHasAnOwner()
    {
        var dto = new CreateRideRequestDTO
        {
            PickupLocation = "123 Main St",
            Destination = "456 Elm St",
            RideCategory = RideCategory.Standard,
            DriverId = Guid.NewGuid()
        };

        var riderId = Guid.NewGuid();
        var command = new RequestRideCommand(dto, riderId);

        var expectedRide = new Ride(dto.PickupLocation, dto.Destination, 50m)
        {
            RiderId = riderId,
            DriverId = dto.DriverId,
            Status = RideStatus.Requested
        };

        var rideFactoryMock = new Mock<IRideFactory>();
        rideFactoryMock
            .Setup(x => x.CreateRide(dto.PickupLocation, dto.Destination, 50m, riderId, dto.DriverId))
            .Returns(expectedRide);

        var geoMock = new Mock<IGeoCodingService>();
        geoMock.Setup(g => g.ConvertAddressToLocationAsync(It.IsAny<string>()))
               .ReturnsAsync(new Location(0, 0, "resolved"));

        var matchingMock = new Mock<IRideMatchingService>();
        matchingMock.Setup(m => m.CalculateFareAsync(It.IsAny<Location>(), It.IsAny<Location>(), It.IsAny<RideCategory>()))
                    .ReturnsAsync(50m);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.RideRepository).Returns(new Mock<IRideRepository>().Object);
        unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var handler = new RequestRideHandler(
            unitOfWorkMock.Object, matchingMock.Object, geoMock.Object, rideFactoryMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.RiderId.Should().Be(riderId);
        rideFactoryMock.Verify(x => x.CreateRide(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), riderId, It.IsAny<Guid?>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReject_WhenTheTokenCarriesNoRider()
    {
        var dto = new CreateRideRequestDTO
        {
            PickupLocation = "123 Main St",
            Destination = "456 Elm St"
        };

        var command = new RequestRideCommand(dto, Guid.Empty);

        var handler = new RequestRideHandler(
            Mock.Of<IUnitOfWork>(),
            Mock.Of<IRideMatchingService>(),
            Mock.Of<IGeoCodingService>(),
            Mock.Of<RideFactory>());

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
