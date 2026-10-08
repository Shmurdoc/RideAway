using FluentAssertions;
using RideAway.Domain.Entities;
using RideAway.Domain.Value_Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideAway.Tests.Domain.Entities
{
    public class RideTests
    {

        [Fact]
        public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
        {
            // Arrange
            string pickup = "Location A";
            string destination = "Location B";
            decimal fare = 50;

            // Act
            var ride = new Ride(pickup, destination, fare);

            // Assert
            ride.PickupLocation.Should().Be(pickup);
            ride.Destination.Should().Be(destination);
            ride.Fare.Should().Be(fare);
            ride.Status.Should().Be(RideStatus.Requested);
        }


        [Fact]
        public void Constructor_WithNegativeFare_ShouldThrowArgumentException()
        {
            // Arrange
            var pickup = "Pickup Location";
            var destination = "Destination";
            var fare = -5m;

            // Act
            Action act = () => new Ride(pickup, destination, fare);

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("Fare must be a non-negative value.*")
               .And.ParamName.Should().Be("fare");
        }



        [Fact]
        public void Constructor_WithNullPickup_ShouldThrowArgumentNullException()
        {
            Action act = () => new Ride(null!, "Destination", 10);
            act.Should().Throw<ArgumentNullException>().WithParameterName("pickup");
        }

        [Fact]
        public void Constructor_WithNullDestination_ShouldThrowArgumentNullException()
        {
            Action act = () => new Ride("Pickup", null!, 10);
            act.Should().Throw<ArgumentNullException>().WithParameterName("destination");
        }

        [Fact]
        public void MarkAsPaid_ShouldSetStatusToPaid_WhenRideIsCompleted()
        {
            // Arrange
            var ride = new Ride("Pickup", "Destination", 100) { Status = RideStatus.Completed };

            // Act
            ride.MarkAsPaid();

            // Assert
            ride.Status.Should().Be(RideStatus.Paid);
        }

        [Fact]
        public void MarkAsPaid_ShouldThrow_WhenRideIsNotCompleted()
        {
            var ride = new Ride("Pickup", "Destination", 100) { Status = RideStatus.Canceled };

            ride.Invoking(r => r.MarkAsPaid())
                .Should().Throw<RideAway.Domain.Exceptions.InvalidRideStatusException>();
        }

        [Fact]
        public void MarkAsPaid_ShouldThrow_WhenRideIsAlreadyPaid()
        {
            var ride = new Ride("Pickup", "Destination", 100) { Status = RideStatus.Completed };
            ride.MarkAsPaid();

            ride.Invoking(r => r.MarkAsPaid())
                .Should().Throw<RideAway.Domain.Exceptions.PaymentProcessingException>();
        }

        [Fact]
        public void Cancel_ShouldThrow_WhenCallerIsNeitherRiderNorDriver()
        {
            var rider = Guid.NewGuid();
            var driver = Guid.NewGuid();
            var stranger = Guid.NewGuid();
            var ride = new Ride("Pickup", "Destination", 100) { RiderId = rider, DriverId = driver };

            ride.Invoking(r => r.Cancel(stranger))
                .Should().Throw<UnauthorizedAccessException>();
        }

        [Fact]
        public void Complete_ShouldThrow_WhenCallerIsNotTheAssignedDriver()
        {
            var ride = new Ride("Pickup", "Destination", 100)
            {
                RiderId = Guid.NewGuid(),
                DriverId = Guid.NewGuid(),
                Status = RideStatus.InProgress
            };

            ride.Invoking(r => r.Complete(Guid.NewGuid()))
                .Should().Throw<UnauthorizedAccessException>();
        }

    }
}
