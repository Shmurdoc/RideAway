using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using RideAway.Application.DTOs;
using RideAway.Application.Features.Payments.Commands;
using RideAway.Application.Features.Rides.Handlers.Commands;
using RideAway.Application.IRepositories;
using RideAway.Domain.Entities;
using RideAway.Domain.Value_Object;
using RideAway.Application.IServices;
using RideAway.Domain.Entities.Enum;
using RideAway.Tests.Moq.Factories;
using RideAway.Tests.Moq.Mocks;
using RideAway.Domain.Exceptions;
namespace RideAway.Tests.Application.Features.Rides.Commands;

public class ProcessPaymentCommandHandlerTests
{
    private readonly Mock<IPaymentProcessingService> _paymentServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<ProcessPaymentCommandHandler>> _loggerMock = new();
    private readonly ProcessPaymentCommandHandler _handler;

    public ProcessPaymentCommandHandlerTests()
    {
        _handler = new ProcessPaymentCommandHandler(_paymentServiceMock.Object, _unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenRideExistsAndPaymentSucceeds_ShouldReturnSuccessfulPaymentResult()
    {
        // Arrange
        var ride = RideFactory.GenerateSingleRide(Guid.NewGuid(), Guid.NewGuid());
        var command = new ProcessPaymentCommand(ride.Id, Guid.NewGuid(), 120m, PaymentMethod.card);
        var paymentResult = PaymentFactory.GetSuccessfulPaymentResult();

        var rides = new List<Ride> { ride };
        var payments = new List<Payment>();

        // Use RideRepositoryMock
        var rideRepositoryMock = RideRepositoryMock.GetMockIRideRepository(rides);

        // Use GenericRepositoryMock for Payment
        var genericRepoMock = GenericRepositoryMock.GetMockRepository(payments);

        // Cast the Generic Mock
        var paymentRepositoryMock = genericRepoMock.As<IPaymentRepository>();

        // Setup UnitOfWork to return those mocks
        _unitOfWorkMock.Setup(u => u.RideRepository).Returns(rideRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.PaymentRepository).Returns(paymentRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        _paymentServiceMock
            .Setup(p => p.ProcessPaymentAsync(command.UserId, command.Amount, command.PaymentMethod))
            .ReturnsAsync(paymentResult);

        var handler = new ProcessPaymentCommandHandler(_paymentServiceMock.Object, _unitOfWorkMock.Object, _loggerMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessful.Should().BeTrue();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        paymentRepositoryMock.Verify(p => p.AddAsync(It.IsAny<Payment>()), Times.Once);
        ride.Status.Should().Be(RideStatus.Completed); // Assert the ride status update
        payments.Should().ContainSingle();         // Assert a payment was added
        payments[0].Amount.Should().Be(command.Amount);
    }

    [Fact]
    public async Task Handle_WhenRideDoesNotExist_ShouldThrowRideNotFoundException()
    {
        // Arrange
        var command = new ProcessPaymentCommand(Guid.NewGuid(), Guid.NewGuid(), 100m, PaymentMethod.card);

        _unitOfWorkMock.Setup(u => u.RideRepository.GetByIdAsync(command.RideId)).ReturnsAsync((Ride?)null);

        var handler = new ProcessPaymentCommandHandler(_paymentServiceMock.Object, _unitOfWorkMock.Object, _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<RideNotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }


    [Fact]
    public async Task Handle_WhenPaymentServiceReturnsNull_ShouldThrowPaymentProcessingException()
    {
        // Arrange
        var ride = RideFactory.GenerateSingleRide(Guid.NewGuid(), Guid.NewGuid());
        var command = new ProcessPaymentCommand(ride.Id, Guid.NewGuid(), 50m, PaymentMethod.card);

        _unitOfWorkMock.Setup(u => u.RideRepository.GetByIdAsync(command.RideId)).ReturnsAsync(ride);
        _paymentServiceMock.Setup(p => p.ProcessPaymentAsync(command.UserId, command.Amount, command.PaymentMethod))
                           .ReturnsAsync((PaymentResultDTO?)null);

        var handler = new ProcessPaymentCommandHandler(_paymentServiceMock.Object, _unitOfWorkMock.Object, _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<PaymentProcessingException>(() => handler.Handle(command, CancellationToken.None));
    }

    
    [Fact]
    public async Task Handle_WhenPaymentFails_ShouldNotSavePaymentOrUpdateRide()
    {
        // Arrange
        var ride = RideFactory.GenerateSingleRide(Guid.NewGuid(), Guid.NewGuid());
        var command = new ProcessPaymentCommand(ride.Id, Guid.NewGuid(), 80m, PaymentMethod.card);
        var failedPaymentResult = new PaymentResultDTO
        {
            IsSuccessful = false,
            PaymentDate = DateTime.UtcNow,
            TransactionReference = "FAILED123",
            FailureReason = "Insufficient funds"
        };

        _unitOfWorkMock.Setup(u => u.RideRepository.GetByIdAsync(command.RideId)).ReturnsAsync(ride);
        _paymentServiceMock.Setup(p => p.ProcessPaymentAsync(command.UserId, command.Amount, command.PaymentMethod))
                           .ReturnsAsync(failedPaymentResult);

        var handler = new ProcessPaymentCommandHandler(_paymentServiceMock.Object, _unitOfWorkMock.Object, _loggerMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.PaymentRepository.AddAsync(It.IsAny<Payment>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.RideRepository.UpdateAsync(It.IsAny<Ride>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
    [Fact]
    public async Task Handle_WhenPaymentSucceeds_ShouldCallUpdateRideWithPaidStatus()
    {
        // Arrange
        var ride = RideFactory.GenerateSingleRide(Guid.NewGuid(), Guid.NewGuid());
        var command = new ProcessPaymentCommand(ride.Id, Guid.NewGuid(), 120m, PaymentMethod.card);
        var paymentResult = PaymentFactory.GetSuccessfulPaymentResult();

        // Create ride repo mock first
        var rideRepositoryMock = new Mock<IRideRepository>();
        rideRepositoryMock.Setup(r => r.GetByIdAsync(command.RideId)).ReturnsAsync(ride);

        _unitOfWorkMock.Setup(u => u.RideRepository).Returns(rideRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.PaymentRepository).Returns(new Mock<IPaymentRepository>().Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        _paymentServiceMock.Setup(p => p.ProcessPaymentAsync(command.UserId, command.Amount, command.PaymentMethod))
                           .ReturnsAsync(paymentResult);

        var handler = new ProcessPaymentCommandHandler(_paymentServiceMock.Object, _unitOfWorkMock.Object, _loggerMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        rideRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Ride>(r => r.Status == RideStatus.Completed)), Times.Once);
    }




}
