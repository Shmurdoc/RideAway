using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using RideAway.Application.DTOs;
using RideAway.Application.Features.Payments.Commands;
using RideAway.Application.Features.Rides.Handlers.Commands;
using RideAway.Application.IRepositories;
using RideAway.Application.IServices;
using RideAway.Domain.Entities;
using RideAway.Domain.Entities.Enum;
using RideAway.Domain.Exceptions;
using RideAway.Domain.Value_Object;

namespace RideAway.Tests.Application.Features.Rides.Commands;

public class ProcessPaymentCommandHandlerTests
{
    private readonly Mock<IPaymentProcessingService> _paymentServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IRideRepository> _rideRepositoryMock = new();
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock = new();
    private readonly ProcessPaymentCommandHandler _handler;

    private readonly Guid _riderId = Guid.NewGuid();
    private readonly Guid _otherUserId = Guid.NewGuid();

    public ProcessPaymentCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.RideRepository).Returns(_rideRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.PaymentRepository).Returns(_paymentRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        _handler = new ProcessPaymentCommandHandler(
            _paymentServiceMock.Object, _unitOfWorkMock.Object,
            Mock.Of<ILogger<ProcessPaymentCommandHandler>>());
    }

    private static Ride CompletedRide(Guid riderId, decimal fare)
    {
        var ride = new Ride("1 Main St", "2 Main St", fare)
        {
            RiderId = riderId,
            Status = RideStatus.Completed
        };
        return ride;
    }

    [Fact]
    public async Task Handle_WhenRideDoesNotExist_ShouldThrowRideNotFoundException()
    {
        var command = new ProcessPaymentCommand(Guid.NewGuid(), _riderId, PaymentMethod.card);
        _rideRepositoryMock.Setup(r => r.GetByIdAsync(command.RideId)).ReturnsAsync((Ride?)null);

        await Assert.ThrowsAsync<RideNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenCallerIsNotTheRider_ShouldReject()
    {
        var ride = CompletedRide(_otherUserId, 100m);
        var command = new ProcessPaymentCommand(ride.Id, _riderId, PaymentMethod.cash);
        _rideRepositoryMock.Setup(r => r.GetByIdAsync(ride.Id)).ReturnsAsync(ride);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None));

        _paymentServiceMock.Verify(
            p => p.CreatePaymentAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<PaymentMethod>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRideIsAlreadyPaid_ShouldRejectToPreventDoubleSettlement()
    {
        var ride = CompletedRide(_riderId, 100m);
        ride.MarkAsPaid();
        var command = new ProcessPaymentCommand(ride.Id, _riderId, PaymentMethod.card);
        _rideRepositoryMock.Setup(r => r.GetByIdAsync(ride.Id)).ReturnsAsync(ride);

        await Assert.ThrowsAsync<PaymentProcessingException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Theory]
    [InlineData(RideStatus.Requested)]
    [InlineData(RideStatus.Accepted)]
    [InlineData(RideStatus.InProgress)]
    [InlineData(RideStatus.Canceled)]
    public async Task Handle_WhenRideIsNotCompleted_ShouldReject(RideStatus status)
    {
        var ride = CompletedRide(_riderId, 100m);
        ride.Status = status;
        var command = new ProcessPaymentCommand(ride.Id, _riderId, PaymentMethod.card);
        _rideRepositoryMock.Setup(r => r.GetByIdAsync(ride.Id)).ReturnsAsync(ride);

        await Assert.ThrowsAsync<InvalidRideStatusException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UsesTheServerComputedFare_NotAnyClientValue()
    {
        var ride = CompletedRide(_riderId, 250.75m);
        var command = new ProcessPaymentCommand(ride.Id, _riderId, PaymentMethod.cash);
        _rideRepositoryMock.Setup(r => r.GetByIdAsync(ride.Id)).ReturnsAsync(ride);

        _paymentServiceMock
            .Setup(p => p.CreatePaymentAsync(ride.Id, _riderId, 250.75m, PaymentMethod.cash))
            .ReturnsAsync(new PaymentResultDTO { IsSuccessful = false, TransactionReference = string.Empty });

        await _handler.Handle(command, CancellationToken.None);

        _paymentServiceMock.Verify(
            p => p.CreatePaymentAsync(ride.Id, _riderId, 250.75m, PaymentMethod.cash),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DoesNotMarkTheRidePaid_WhenPaymentIsOnlyPending()
    {
        var ride = CompletedRide(_riderId, 100m);
        var command = new ProcessPaymentCommand(ride.Id, _riderId, PaymentMethod.cash);
        _rideRepositoryMock.Setup(r => r.GetByIdAsync(ride.Id)).ReturnsAsync(ride);

        _paymentServiceMock
            .Setup(p => p.CreatePaymentAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<PaymentMethod>()))
            .ReturnsAsync(new PaymentResultDTO { IsSuccessful = false, TransactionReference = string.Empty });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        ride.Status.Should().Be(RideStatus.Completed);

        _rideRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Ride>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRideHasNoPayableFare_ShouldReject()
    {
        var ride = CompletedRide(_riderId, 0m);
        var command = new ProcessPaymentCommand(ride.Id, _riderId, PaymentMethod.cash);
        _rideRepositoryMock.Setup(r => r.GetByIdAsync(ride.Id)).ReturnsAsync(ride);

        await Assert.ThrowsAsync<PaymentProcessingException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenPaymentServiceReturnsNull_ShouldThrowPaymentProcessingException()
    {
        var ride = CompletedRide(_riderId, 50m);
        var command = new ProcessPaymentCommand(ride.Id, _riderId, PaymentMethod.card);
        _rideRepositoryMock.Setup(r => r.GetByIdAsync(ride.Id)).ReturnsAsync(ride);

        _paymentServiceMock
            .Setup(p => p.CreatePaymentAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<PaymentMethod>()))
            .ReturnsAsync((PaymentResultDTO)null!);

        await Assert.ThrowsAsync<PaymentProcessingException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenTokenCarriesNoRider_ShouldNotSettleTheRide()
    {
        var ride = CompletedRide(Guid.NewGuid(), 100m);
        var command = new ProcessPaymentCommand(ride.Id, Guid.Empty, PaymentMethod.cash);
        _rideRepositoryMock.Setup(r => r.GetByIdAsync(ride.Id)).ReturnsAsync(ride);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None));

        _paymentServiceMock.Verify(
            p => p.CreatePaymentAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<PaymentMethod>()),
            Times.Never);
    }
}
