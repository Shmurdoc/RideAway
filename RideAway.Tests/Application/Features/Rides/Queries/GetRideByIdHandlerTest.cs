using FluentAssertions;
using RideAway.Tests.Moq;

namespace RideAway.Tests.Application.Features.Rides.Queries
{
    public class GetRideByIdHandlerTest
    {

        [Fact]
        public async Task GetByIdAsync_ShouldReturnRide_WhenRideExists()
        {
            // Arrange
            var (unitOfWork, _, expectedRide, _) = MockRideRepository.GetByIdUnitOfWork();

            // Act
            var result = await unitOfWork.RideRepository.GetByIdAsync(expectedRide.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(expectedRide.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenRideDoesNotExist()
        {
            // Arrange
            var (unitOfWork, _, _, notFoundGuid) = MockRideRepository.GetByIdUnitOfWork();

            // Act
            var result = await unitOfWork.RideRepository.GetByIdAsync(notFoundGuid);

            // Assert
            result.Should().BeNull();
        }


    }
}
