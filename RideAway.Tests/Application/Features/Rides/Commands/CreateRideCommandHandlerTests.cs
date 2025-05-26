using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RideAway.Application.Features.Rides.Commands;
using RideAway.Application.Features.Rides.Handlers.Commands;
using RideAway.Application.IRepositories;
using RideAway.Domain.Entities;
using RideAway.Domain.Entities.Enum;

namespace RideAway.Tests.Application.Features.Rides.Commands;

public class CreateUserHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateUserSuccessfully()
    {
        // Arrange
        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(u => u.UserRepository).Returns(mockUserRepo.Object);
        mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var mockLogger = new Mock<ILogger<CreateUserHandler>>();

        var handler = new CreateUserHandler(mockUnitOfWork.Object, mockLogger.Object);

        // Ensure expected values align during setup
        var command = new CreateUserCommand(new RideAway.Application.DTOs.CreateUserDTO
        {
            Name = "John Snow", // Updated to match test assertion
            Role = UserRole.Driver
        });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("John Snow"); // Matches the value in the command setup
        result.Role.Should().Be(UserRole.Driver);

        // Verify mock interactions
        mockUserRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("New user created")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }

}
