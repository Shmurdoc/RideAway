using System.Net;
using Microsoft.Extensions.Configuration;
using RideAway.Application.Services;
using FluentAssertions;
using Moq.Protected;
using Moq;

namespace RideAway.Tests.Application.Services
{
    public class GoogleMapsApiServiceTests
    {
        private GoogleMapsApiService CreateService(string jsonResponse)
        {
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse),
                });

            var httpClient = new HttpClient(handlerMock.Object);

            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["GoogleMaps:ApiKey"]).Returns("fake-api-key");

            return new GoogleMapsApiService(httpClient, configMock.Object);
        }

        [Fact]
        public async Task CalculateDistanceAsync_ReturnsCorrectDistanceInKm()
        {
            // Arrange: Fake JSON from Google Distance Matrix API
            var jsonResponse = @"{
              'rows': [
                {
                  'elements': [
                    {
                      'distance': {
                        'value': 12340
                      }
                    }
                  ]
                }
              ]
            }".Replace('\'', '"');

            var service = CreateService(jsonResponse);

            // Act
            var distance = await service.CalculateDistanceAsync(1, 2, 3, 4);

            // Assert
            distance.Should().Be(12.34);
        }
    }
}
