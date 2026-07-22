using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using RideAway.Application.DTOs;
using RideAway.Application.Features.Rides.Commands;
using RideAway.Application.Features.Rides.Handlers.Commands;
using RideAway.Application.IRepositories;
using RideAway.Domain.Entities;
using RideAway.Domain.Value_Object;
using RideAway.Tests.Moq.Factories;
using Xunit;

namespace RideAway.Tests.Application.Features.Rides.Commands;

public class UpdateDriverLocationHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateDriverLocation_WhenDriverExists()
    {
        // Arrange
        var driverId = Guid.NewGuid();
        var newLocation = "7 Pres Brand Phalaborwa"; // South Africa Limpopo

        var driver = UserFactory.GenerateDriver();

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(r => r.GetByIdAsync(driverId)).ReturnsAsync(driver);

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(u => u.UserRepository).Returns(mockUserRepo.Object);
        mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var handler = new UpdateDriverLocationHandler(mockUnitOfWork.Object);
        
        var command = new UpdateDriverLocationCommand(new DriverLocationUpdateDTO
        {
            Id = driverId,
            CurrentLocation = newLocation
        });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        driver.CurrentLocation.Should().BeEquivalentTo(newLocation);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenDriverNotFound()
    {
        // Arrange
        var driverId = Guid.NewGuid();

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(r => r.GetByIdAsync(driverId)).ReturnsAsync((User)null!);

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(u => u.UserRepository).Returns(mockUserRepo.Object);

        var handler = new UpdateDriverLocationHandler(mockUnitOfWork.Object);

        var command = new UpdateDriverLocationCommand(new DriverLocationUpdateDTO
        {
            Id = driverId,
            CurrentLocation = "12 Mopani Avenue, Phalaborwa, Limpopo, South Africa, 1390"
        });

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Driver not found.");
    }
}
