using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RideAway.API.Controllers;
using RideAway.Application.DTOs;
using RideAway.Application.Features.Authentication.Commands;
using RideAway.Application.IServices.IAuthentication;
using RideAway.Domain.Entities;
using RideAway.Domain.Entities.Enum;

namespace RideAway.Tests.API.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _controller = new AuthController(_mediatorMock.Object, _jwtTokenGeneratorMock.Object);
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var user = new User
        {
            Name = "Test Rider",
            Email = "rider@example.com",
            Role = UserRole.Rider
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), default)).ReturnsAsync(user);
        _jwtTokenGeneratorMock.Setup(t => t.GenerateToken(user)).Returns("fake-jwt-token");

        var result = await _controller.Login(new LoginCommand("rider@example.com", "P@ssw0rd!"));

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<LoginResponseDTO>(okResult.Value);
        response.Token.Should().Be("fake-jwt-token");
        response.UserId.Should().Be(user.Id);
        response.Role.Should().Be("Rider");
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenCommandIsNull()
    {
        var result = await _controller.Login(null!);

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
