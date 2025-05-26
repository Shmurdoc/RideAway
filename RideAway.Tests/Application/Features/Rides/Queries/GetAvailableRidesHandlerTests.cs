using AutoMapper;
using FluentAssertions;
using Moq;
using RideAway.Application.DTOs;
using RideAway.Application.Features.Rides.Handlers.Queries;
using RideAway.Application.Features.Rides.Queries;
using RideAway.Application.IRepositories;
using RideAway.Application.IServices;
using RideAway.Domain.Entities;
using RideAway.Domain.Entities.Enum;
using RideAway.Infrastructure.Mappers;
using RideAway.Tests.Moq.Factories;
using System.Linq.Expressions;

namespace RideAway.Tests.Application.Features.Rides.Queries
{
    public class GetAvailableRidesHandlerTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IRideRepository> _mockRideRepository;
        private const string PickupLocation = "pickup";
        private const string DropoffLocation = "dropoff";

        public GetAvailableRidesHandlerTests()
        {
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<RideProfile>();
            });

            _mapper = mapperConfig.CreateMapper();
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoMatchingRidesFound()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>(); // ✅ Added this
            var mockMapper = new Mock<IMapper>();
            var mockRideMatchingService = new Mock<IRideMatchingService>();
            var mockLocationService = new Mock<ILocationService>();
            var mockGeoCodingService = new Mock<IGeoCodingService>();
            var generateRideAlias = RideFactory.GenerateRideAlias();

            // Simulate no matching rides
            mockRideMatchingService
                .Setup(service => service.FindDriverAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<RideCategory>()))
                .ReturnsAsync(new List<RideDTO>());

            var handler = new GetAvailableRidesHandler(
                mockMapper.Object,
                mockRideMatchingService.Object,
                mockLocationService.Object,
                mockGeoCodingService.Object
            );

            var query = new GetAvailableRidesQuery(generateRideAlias.PickupLocation!, generateRideAlias.Destination!, generateRideAlias.RiderCategory);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty(); // No rides should be found
            result.Should().BeOfType<List<RideDTO>>();
        }



        [Fact]
        public async Task Handle_ShouldReturnRides_WhenMatchingRidesExist()
        {
            // Arrange
            //Generate Ride List
            var testRides = RideFactory.GenerateRides(5);

            var rideRepoMock = new Mock<IRideRepository>();
            rideRepoMock.As<IGenericRepository<Ride>>().SetupAllProperties();

            rideRepoMock.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Ride, bool>>>(), It.IsAny<string?>())).ReturnsAsync(testRides);
            

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            //// Inject RideRepository mock
            //unitOfWorkMock.Setup(uow => uow.RideRepository).Returns(rideRepoMock.Object);
           
            //// Optionally inject SaveChangesAsync behavior
            //unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);
            
            var expectedRide = await rideRepoMock.Object.GetAllAsync(null); // Fetch the generated ride
            var singleRide = expectedRide.First();

            var mockRideMatchingService = new Mock<IRideMatchingService>();

            var expectedRides = new List<RideDTO>
    {
        new()
        {
            DriverId = singleRide!.DriverId!,
            PickupLocation = singleRide.PickupLocation,
            Destination = singleRide.Destination,
            EstimatedFare = singleRide.Fare
        }
    };

            mockRideMatchingService
                .Setup(service => service.FindDriverAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RideCategory>()))
                .ReturnsAsync(expectedRides);

            var handler = new GetAvailableRidesHandler(
                _mapper,
                mockRideMatchingService.Object,
                Mock.Of<ILocationService>(),
                Mock.Of<IGeoCodingService>()
            );

            var query = new GetAvailableRidesQuery(singleRide.PickupLocation, singleRide.Destination, RideCategory.Standard);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().ContainSingle();
            result[0].DriverId.Should().Be(singleRide.DriverId);
        }


    }
}
