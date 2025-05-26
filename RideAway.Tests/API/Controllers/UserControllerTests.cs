using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using RideAway.Application.Features.Rides.Commands;
using RideAway.Application.Features.Payments.Commands;
using RideAway.Application.Features.Rides.Queries;
using RideAway.Application.DTOs;
using RideAway.Domain.Entities.Enum;
using RideAway.Domain.Exceptions;
using RideAway.API.Controllers.RideAway.WebAPI.Controllers;
using RideAway.Domain.Entities;
using RideAway.Tests.Moq.Factories;
using RideAway.Tests.Moq;
using RideAlias = RideAway.Domain.Entities.Ride;
using Moq;
using Newtonsoft.Json;
using FluentAssertions;

namespace RideAway.Tests.API.Controllers;

public class UserControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly Mock<ILogger<UserController>> _loggerMock = new();
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _controller = new UserController(_mediatorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateUser_ReturnsOk_WhenUserCreated()
    {
        var userDto = UserFactory.GenerateUserDTO(UserRole.Driver);
        var command = new CreateUserCommand(userDto);

        var expectedUser = new User { Name = userDto.Name, Role = userDto.Role };

        _mediatorMock.Setup(m => m.Send(It.Is<CreateUserCommand>(c =>
            c.createUserDTO.Name == userDto.Name && c.createUserDTO.Role == userDto.Role), default))
            .ReturnsAsync(expectedUser);

        var result = await _controller.CreateUser(command);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedUser = Assert.IsType<User>(okResult.Value);
        Assert.Equal(expectedUser.Name, returnedUser.Name);
        Assert.Equal(expectedUser.Role, returnedUser.Role);
    }

    [Fact]
    public async Task GetUserById_ReturnsNotFound_WhenUserIsNull()
    {
        var userId = Guid.NewGuid();

        _mediatorMock.Setup(m => m.Send(It.Is<GetUserByIdQuery>(q => q.Guid == userId), default))
                     .ReturnsAsync((User?)null);

        var result = await _controller.GetUserById(userId);

        Assert.IsType<NotFoundObjectResult>(result);
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
    public async Task CancelRide_ReturnsOk_WhenSuccess()
    {
        // Arrange
        var rideId = Guid.NewGuid();
        var command = new CancelRideCommand(rideId);

        _mediatorMock.Setup(m => m.Send(command, default)).ReturnsAsync(true);

        // Act
        var result = await _controller.CancelRide(command);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);

        // Convert the anonymous object to a dictionary using serialization
        var serialized = JsonConvert.SerializeObject(okResult.Value);
        var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(serialized);

        Assert.Equal("Ride canceled successfully.", response["Message"]);
    }

    [Fact]
    public async Task CancelRide_ReturnsNotFound_WhenRideNotFound()
    {
        // Arrange
        var rideId = Guid.NewGuid();
        var command = new CancelRideCommand(rideId);

        _mediatorMock.Setup(m => m.Send(It.IsAny<CancelRideCommand>(), default))
            .ThrowsAsync(new RideNotFoundException("Ride not found."));

        // Act
        var result = await _controller.CancelRide(command);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;

        // Deserialize anonymous object to Dictionary
        var json = System.Text.Json.JsonSerializer.Serialize(notFoundResult.Value);
        var response = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);

        response.Should().ContainKey("Message");
        response!["Message"].Should().Be("Ride not found.");
    }




    [Fact]
    public async Task CancelRide_ReturnsBadRequest_WhenRideAlreadyCompleted()
    {
        // Arrange
        var rideId = Guid.NewGuid();
        var command = new CancelRideCommand(rideId);

        _mediatorMock.Setup(m => m.Send(It.IsAny<CancelRideCommand>(), default))
            .ThrowsAsync(new InvalidRideStatusException("Ride has already been completed and cannot be canceled."));

        // Act
        var result = await _controller.CancelRide(command);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

        // Safely convert the anonymous object to a known type
        var json = System.Text.Json.JsonSerializer.Serialize(badRequestResult.Value);
        var response = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);

        Assert.NotNull(response);
        Assert.True(response!.ContainsKey("Message"));
        Assert.Equal("Ride has already been completed and cannot be canceled.", response["Message"]);
    }


    [Fact]
    public async Task ProcessPayment_ReturnsOkResult_WhenPaymentIsSuccessful()
    {
        var ride = MockPaymentRepository.GetFakeRide();
        var fakeCommand = MockPaymentRepository.GetFakeCommand(rideId: ride.Id);

        var command = new ProcessPaymentCommand(
            fakeCommand.RideId,
            fakeCommand.UserId,
            fakeCommand.Amount,
            fakeCommand.PaymentMethod
        );

        var paymentResult = new PaymentResultDTO
        {
            IsSuccessful = true,
            TransactionReference = Guid.NewGuid().ToString(),
            PaymentDate = DateTime.UtcNow
        };

        _mediatorMock
            .Setup(m => m.Send(It.Is<ProcessPaymentCommand>(c =>
                c.RideId == command.RideId &&
                c.UserId == command.UserId &&
                c.Amount == command.Amount &&
                c.PaymentMethod == command.PaymentMethod
            ), default))
            .ReturnsAsync(paymentResult);

        var result = await _controller.ProcessPayment(command);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedDto = Assert.IsType<PaymentResultDTO>(okResult.Value);

        Assert.True(returnedDto.IsSuccessful);
        Assert.Equal(paymentResult.TransactionReference, returnedDto.TransactionReference);
    }

    [Fact]
    public async Task RequestRide_ReturnsBadRequest_WhenDtoIsNull()
    {
        var result = await _controller.RequestRide(null);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Ride request cannot be null.", badRequestResult.Value);
    }

    [Fact]
    public async Task RequestRide_ReturnsOk_WhenSuccessful()
    {
        var dto = RideFactory.GenerateRideRequestDTO();
        var expectedRide = RideFactory.GenerateRideAlias();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<RequestRideCommand>(), default))
            .ReturnsAsync(expectedRide);

        var result = await _controller.RequestRide(dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedRide = Assert.IsType<RideAlias>(okResult.Value);
        Assert.Equal(expectedRide.DriverId, returnedRide.DriverId);
        Assert.Equal(expectedRide.PickupLocation, returnedRide.PickupLocation);
        Assert.Equal(expectedRide.Destination, returnedRide.Destination);
        Assert.Equal(expectedRide.RiderCategory, returnedRide.RiderCategory);
    }
}
