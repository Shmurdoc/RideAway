using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RideAway.Application.Services;
using RichardSzalay.MockHttp;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace RideAway.Tests.Application.Services
{
    public class GoogleGeocodingServiceTests
    {
        private readonly GoogleGeocodingService _service;
        private readonly MockHttpMessageHandler _mockHttp;
        private readonly Mock<IConfiguration> _configuration;
        private readonly Mock<ILogger<GoogleGeocodingService>> _logger;

        public GoogleGeocodingServiceTests()
        {
            _mockHttp = new MockHttpMessageHandler();
            var httpClient = new HttpClient(_mockHttp);
            _configuration = new Mock<IConfiguration>();
            _logger = new Mock<ILogger<GoogleGeocodingService>>();

            _configuration.Setup(c => c["GoogleMaps:ApiKey"]).Returns("dummy-key");

            _service = new GoogleGeocodingService(httpClient, _configuration.Object, _logger.Object);
        }

        [Fact]
        public async Task ConvertAddressToLocationAsync_ReturnsCorrectLocation()
        {
            // Arrange
            var mockResponse = @"
            {
              ""results"": [
                {
                  ""geometry"": {
                    ""location"": {
                      ""lat"": -23.94,
                      ""lng"": 31.14
                    }
                  }
                }
              ],
              ""status"": ""OK""
            }";

            _mockHttp.When("https://maps.googleapis.com/maps/api/geocode/json*")
         .Respond("application/json", mockResponse);


            // Act
            var location = await _service.ConvertAddressToLocationAsync("1 Starling St");

            // Assert
            location.Should().NotBeNull();
            location.Coordinates.Should().NotBeNull();
            location.Coordinates!.Latitude.Should().Be(-23.94);
            location.Coordinates!.Longitude.Should().Be(31.14);
        }
    }
}
