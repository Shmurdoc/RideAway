using RideAway.Application.IServices;
using RideAway.Application.Services;
using RideAway.Domain.Entities;
using RideAway.Domain.Value_Object;
using Moq;
using Xunit;
using FluentAssertions;
using System;
using System.Threading.Tasks;

namespace RideAway.Tests.Application.Services
{
    public class GoogleMapsLocationServiceTests
    {
        private readonly Mock<IGoogleMapsApi> _googleMapsApiMock;
        private readonly GoogleMapsLocationService _service;

        public GoogleMapsLocationServiceTests()
        {
            _googleMapsApiMock = new Mock<IGoogleMapsApi>();
            _service = new GoogleMapsLocationService(_googleMapsApiMock.Object);
        }

        [Fact]
        public async Task GetDistanceAsync_ShouldCallCalculateDistanceAsyncAndReturnValue()
        {
            // Arrange
            var origin = new Location(1.0, 2.0, "Origin Address");
            var destination = new Location(3.0, 4.0, "Destination Address");
            var expectedDistance = 10.5;

            _googleMapsApiMock.Setup(api =>
                api.CalculateDistanceAsync(origin.Coordinates.Latitude, origin.Coordinates.Longitude,
                                           destination.Coordinates.Latitude, destination.Coordinates.Longitude))
                .ReturnsAsync(expectedDistance);

            // Act
            var result = await _service.GetDistanceAsync(origin, destination);

            // Assert
            result.Should().Be(expectedDistance);

            _googleMapsApiMock.Verify(api =>
                api.CalculateDistanceAsync(origin.Coordinates.Latitude, origin.Coordinates.Longitude,
                                           destination.Coordinates.Latitude, destination.Coordinates.Longitude),
                Times.Once);
        }
    }
}
