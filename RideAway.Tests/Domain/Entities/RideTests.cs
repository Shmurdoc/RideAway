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
        public void MarkAsPaid_ShouldSetStatusToCompleted()
        {
            // Arrange
            var ride = new Ride("Pickup", "Destination", 100);

            // Act
            ride.MarkAsPaid();

            // Assert
            ride.Status.Should().Be(RideStatus.Completed);
        }

    }
}
