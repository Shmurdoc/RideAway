using FluentAssertions;
using Moq;
using RideAway.Application.Features.Authentication.Commands;
using RideAway.Application.Features.Authentication.Handlers;
using RideAway.Application.IRepositories;
using RideAway.Application.Services;
using RideAway.Domain.Entities;
using RideAway.Domain.Entities.Enum;
using RideAway.Domain.Exceptions;

namespace RideAway.Tests.Application.Features.Authentication;

public class LoginHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUnitOfWork.Setup(u => u.UserRepository).Returns(_mockUserRepository.Object);
        _handler = new LoginHandler(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnUser_WhenCredentialsAreValid()
    {
        var user = new User
        {
            Email = "rider@example.com",
            Name = "Test Rider",
            Role = UserRole.Rider,
            PasswordHash = PasswordHasher.Hash("P@ssw0rd!")
        };
        _mockUserRepository.Setup(r => r.Get(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), null, true))
            .ReturnsAsync(user);

        var result = await _handler.Handle(new LoginCommand("rider@example.com", "P@ssw0rd!"), CancellationToken.None);

        result.Should().NotBeNull();
        result.Email.Should().Be("rider@example.com");
        result.Role.Should().Be(UserRole.Rider);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenPasswordIsWrong()
    {
        var user = new User
        {
            Email = "rider@example.com",
            PasswordHash = PasswordHasher.Hash("P@ssw0rd!")
        };
        _mockUserRepository.Setup(r => r.Get(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), null, true))
            .ReturnsAsync(user);

        var act = async () => await _handler.Handle(new LoginCommand("rider@example.com", "wrong"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUserDoesNotExist()
    {
        _mockUserRepository.Setup(r => r.Get(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), null, true))
            .ReturnsAsync((User?)null);

        var act = async () => await _handler.Handle(new LoginCommand("nobody@example.com", "P@ssw0rd!"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }
}
