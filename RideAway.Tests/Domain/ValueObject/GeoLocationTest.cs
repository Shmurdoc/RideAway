using RideAway.Domain.Value_Object;
using FluentAssertions;
using System;
using Xunit;

namespace RideAway.Tests.Domain.Value_Object;
public class GeoLocationTests
{
    [Fact]
    public void Constructor_WithValidCoordinates_ShouldCreateObject()
    {
        // Arrange
        var latitude = 45.0;
        var longitude = 90.0;

        // Act
        var location = new GeoLocation(latitude, longitude);

        // Assert
        location.Latitude.Should().Be(latitude);
        location.Longitude.Should().Be(longitude);
    }

    [Theory]
    [InlineData(-91, 0)]
    [InlineData(91, 0)]
    public void Constructor_WithInvalidLatitude_ShouldThrowArgumentException(double latitude, double longitude)
    {
        // Act
        Action act = () => new GeoLocation(latitude, longitude);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("Latitude must be between -90 and 90 degrees.");
    }

    [Theory]
    [InlineData(0, -181)]
    [InlineData(0, 181)]
    public void Constructor_WithInvalidLongitude_ShouldThrowArgumentException(double latitude, double longitude)
    {
        // Act
        Action act = () => new GeoLocation(latitude, longitude);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("Longitude must be between -180 and 180 degrees.");
    }

    [Fact]
    public void Equals_WithSameCoordinates_ShouldReturnTrue()
    {
        // Arrange
        var loc1 = new GeoLocation(10.123, 20.456);
        var loc2 = new GeoLocation(10.123, 20.456);

        // Act & Assert
        loc1.Equals(loc2).Should().BeTrue();
        loc1.Should().Be(loc2);
    }

    [Fact]
    public void Equals_WithDifferentCoordinates_ShouldReturnFalse()
    {
        var loc1 = new GeoLocation(10.123, 20.456);
        var loc2 = new GeoLocation(11.000, 21.000);

        loc1.Equals(loc2).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_ForEqualObjects_ShouldBeSame()
    {
        var loc1 = new GeoLocation(10.0, 20.0);
        var loc2 = new GeoLocation(10.0, 20.0);

        loc1.GetHashCode().Should().Be(loc2.GetHashCode());
    }
}
