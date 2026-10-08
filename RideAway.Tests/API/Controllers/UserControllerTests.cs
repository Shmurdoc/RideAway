using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RideAway.API.Controllers;
using RideAway.Application.DTOs;
using RideAway.Application.Features.Payments.Commands;
using RideAway.Application.Features.Rides.Commands;
using RideAway.Application.Features.Rides.Queries;
using RideAway.Domain.Entities;
using RideAway.Domain.Entities.Enum;
using RideAway.Domain.Exceptions;
using RideAway.Tests.Moq;
using RideAway.Tests.Moq.Factories;
using Moq;
using FluentAssertions;

namespace RideAway.Tests.API.Controllers;

public class UserControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly UserController _controller;
    private readonly Guid _callerId = Guid.NewGuid();

    public UserControllerTests()
    {
        _controller = new UserController(_mediatorMock.Object);
        AuthenticateAs(_callerId, UserRole.Rider);
    }

    private void AuthenticateAs(Guid userId, UserRole role)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role.ToString())
        }, "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task CreateUser_ReturnsProfileWithoutPasswordHash()
    {
        var userDto = UserFactory.GenerateUserDTO(UserRole.Driver);
        var command = new CreateUserCommand(userDto);

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateUserCommand>(), default))
            .ReturnsAsync(new UserProfileDTO
            {
                Id = Guid.NewGuid(),
                Name = userDto.Name,
                Email = userDto.Email,
                Role = UserRole.Driver
            });

        var result = await _controller.CreateUser(command);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<UserProfileDTO>(okResult.Value);

        var serialized = JsonConvert.SerializeObject(okResult.Value);
        serialized.Should().NotContain("PasswordHash");
        returned.Email.Should().Be(userDto.Email);
    }

    [Fact]
    public async Task GetUserById_ReturnsNotFound_WhenUserIsNull()
    {
        var userId = _callerId;

        _mediatorMock.Setup(m => m.Send(It.Is<GetUserByIdQuery>(q => q.Id == userId), default))
                     .ReturnsAsync((UserProfileDTO?)null);

        var result = await _controller.GetUserById(userId);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetUserById_PassesAuthenticatedCaller_NotTheRouteId()
    {
        var routeId = Guid.NewGuid();
        GetUserByIdQuery? captured = null;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserByIdQuery>(), default))
            .Callback<IRequest<UserProfileDTO>, CancellationToken>((req, _) => captured = (GetUserByIdQuery)req)
            .ReturnsAsync((UserProfileDTO?)null);

        await _controller.GetUserById(routeId);

        captured.Should().NotBeNull();
        captured!.RequesterId.Should().Be(_callerId);
        captured.Id.Should().Be(routeId);
        captured.RequesterIsAdmin.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserById_MarksAdminCaller_SoAdminsCanReadOthers()
    {
        AuthenticateAs(Guid.NewGuid(), UserRole.Admin);
        GetUserByIdQuery? captured = null;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserByIdQuery>(), default))
            .Callback<IRequest<UserProfileDTO>, CancellationToken>((req, _) => captured = (GetUserByIdQuery)req)
            .ReturnsAsync((UserProfileDTO?)null);

        await _controller.GetUserById(Guid.NewGuid());

        captured!.RequesterIsAdmin.Should().BeTrue();
    }

    [Fact]
    public async Task GetAvailableRides_ReturnsOk_WithRideList()
    {
        var ridesList = RideFactory.GenerateRides(2);
        var mockRides = ridesList.Select(ride => new RideDTO
        {
            PickupLocation = ride.PickupLocation!,
            Destination = ride.Destination!,
            EstimatedFare = ride.Fare,
            Status = ride.Status
        }).ToList();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAvailableRidesQuery>(), default))
                     .ReturnsAsync(mockRides);

        var result = await _controller.GetAvailableRides("A", "B", RideCategory.Standard);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var rides = Assert.IsAssignableFrom<List<RideDTO>>(okResult.Value);
        Assert.Equal(2, rides.Count);
    }

    [Fact]
    public async Task CancelRide_UsesAuthenticatedCallerAsRequester()
    {
        var rideId = Guid.NewGuid();
        var command = new CancelRideCommand(rideId, Guid.Empty);
        CancelRideCommand? captured = null;

        _mediatorMock.Setup(m => m.Send(It.IsAny<CancelRideCommand>(), default))
            .Callback<IRequest<bool>, CancellationToken>((req, _) => captured = (CancelRideCommand)req)
            .ReturnsAsync(true);

        var result = await _controller.CancelRide(command);

        Assert.IsType<OkObjectResult>(result);
        captured!.RequesterId.Should().Be(_callerId);
    }

    [Fact]
    public async Task CancelRide_ReturnsNotFound_WhenRideNotFound()
    {
        var command = new CancelRideCommand(Guid.NewGuid(), Guid.Empty);

        _mediatorMock.Setup(m => m.Send(It.IsAny<CancelRideCommand>(), default))
            .ThrowsAsync(new RideNotFoundException("Ride not found."));

        await Assert.ThrowsAsync<RideNotFoundException>(() => _controller.CancelRide(command));
    }

    [Fact]
    public async Task CancelRide_ReturnsBadRequest_WhenRideAlreadyCompleted()
    {
        var command = new CancelRideCommand(Guid.NewGuid(), Guid.Empty);

        _mediatorMock.Setup(m => m.Send(It.IsAny<CancelRideCommand>(), default))
            .ThrowsAsync(new InvalidRideStatusException("Ride has already been completed and cannot be canceled."));

        await Assert.ThrowsAsync<InvalidRideStatusException>(() => _controller.CancelRide(command));
    }

    [Fact]
    public async Task ProcessPayment_UsesAuthenticatedCaller_AndSendsNoAmount()
    {
        var command = new ProcessPaymentCommand(Guid.NewGuid(), Guid.Empty, PaymentMethod.cash);
        ProcessPaymentCommand? captured = null;

        _mediatorMock.Setup(m => m.Send(It.IsAny<ProcessPaymentCommand>(), default))
            .Callback<IRequest<PaymentResultDTO>, CancellationToken>((req, _) => captured = (ProcessPaymentCommand)req)
            .ReturnsAsync(new PaymentResultDTO { IsSuccessful = false, TransactionReference = "cs_test_123" });

        var result = await _controller.ProcessPayment(command);

        Assert.IsType<OkObjectResult>(result);
        captured!.RiderId.Should().Be(_callerId);

        typeof(ProcessPaymentCommand).GetProperties()
            .Should().NotContain(p => p.Name == "Amount");
        typeof(ProcessPaymentCommand).GetProperties()
            .Should().NotContain(p => p.Name == "UserId");
    }

    [Fact]
    public async Task ProcessPayment_ReportsPending_NotSuccess()
    {
        var command = new ProcessPaymentCommand(Guid.NewGuid(), Guid.Empty, PaymentMethod.cash);

        _mediatorMock.Setup(m => m.Send(It.IsAny<ProcessPaymentCommand>(), default))
            .ReturnsAsync(new PaymentResultDTO { IsSuccessful = false, TransactionReference = string.Empty });

        var result = await _controller.ProcessPayment(command);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var json = JsonConvert.SerializeObject(okResult.Value);
        json.Should().Contain("pending");
    }

    [Fact]
    public async Task RequestRide_ReturnsBadRequest_WhenDtoIsNull()
    {
        var result = await _controller.RequestRide(null);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Ride request cannot be null.", badRequestResult.Value);
    }

    [Fact]
    public async Task RequestRide_RecordsAuthenticatedCallerAsRider()
    {
        var dto = RideFactory.GenerateRideRequestDTO();
        RequestRideCommand? captured = null;

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<RequestRideCommand>(), default))
            .Callback<IRequest<Ride>, CancellationToken>((req, _) => captured = (RequestRideCommand)req)
            .ReturnsAsync(RideFactory.GenerateRideAlias());

        await _controller.RequestRide(dto);

        captured!.RiderId.Should().Be(_callerId);
    }
}
