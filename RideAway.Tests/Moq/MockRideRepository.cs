using Bogus;
using Moq;
using RideAway.Application.IRepositories;
using RideAway.Domain.Entities;
using RideAway.Domain.Entities.Enum;
using RideAway.Domain.Value_Object;
using System.Linq.Expressions;


namespace RideAway.Tests.Moq {


    public static class MockRideRepository
    {

        // Generate fake Ride data
        static List<Ride> GenerateItems(int numberOfItems)
        {
            var itemGenerator = new Faker<Ride>()
                .RuleFor(x => x.Id, f => f.Random.Guid())
                .RuleFor(x => x.DriverId, f => f.Random.Guid())
                .RuleFor(x => x.PickupLocation, f => f.Address.FullAddress())
                .RuleFor(x => x.Destination, f => f.Address.FullAddress())
                .RuleFor(x => x.Fare, f => f.Random.Decimal(10, 200))
                .RuleFor(x => x.RiderCategory, f => f.PickRandom<RideCategory>());

            return itemGenerator.Generate(numberOfItems);
        }

        public static (IUnitOfWork UnitOfWork, Mock<IRideRepository> RideRepoMock) GetAllUnitOfWork(int numberOfItems)
        {
            // Mock IRideRepository
            var mockRideRepository = new Mock<IRideRepository>();

            mockRideRepository
                .Setup(repo => repo.GetAllAsync(It.IsAny<Expression<Func<Ride, bool>>>(), It.IsAny<string?>()))
                .ReturnsAsync(GenerateItems(numberOfItems));

            // Mock IUnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork
                .Setup(uow => uow.RideRepository)
                .Returns(mockRideRepository.Object);

            return (mockUnitOfWork.Object, mockRideRepository);
        }



        public static (IUnitOfWork UnitOfWork, Mock<IRideRepository> RideRepoMock, Ride Ride, Guid NotFoundGuid) GetByIdUnitOfWork()
        {
            // Create a known GUID that will return a ride
            var foundGuid = Guid.NewGuid();

            // Create another GUID that will simulate "not found"
            var notFoundGuid = Guid.NewGuid();

            // Generate a ride with the foundGuid
            var ride = new Faker<Ride>()
                .RuleFor(x => x.Id, _ => foundGuid)
                .RuleFor(x => x.DriverId, f => f.Random.Guid())
                .RuleFor(x => x.PickupLocation, f => f.Address.FullAddress())
                .RuleFor(x => x.Destination, f => f.Address.FullAddress())
                .RuleFor(x => x.Fare, f => f.Random.Decimal(10, 200))
                .RuleFor(x => x.RiderCategory, f => f.PickRandom<RideCategory>())
                .Generate();

            // Mock repository
            var mockRideRepository = new Mock<IRideRepository>();
            mockRideRepository
                .Setup(repo => repo.GetByIdAsync(foundGuid))
                .ReturnsAsync(ride);

            mockRideRepository
                .Setup(repo => repo.GetByIdAsync(notFoundGuid))
                .ReturnsAsync((Ride?)null); // Simulate not found

            // Mock unit of work
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork
                .Setup(uow => uow.RideRepository)
                .Returns(mockRideRepository.Object);

            return (mockUnitOfWork.Object, mockRideRepository, ride, notFoundGuid);
        }

        public static (IUnitOfWork UnitOfWork, Ride Ride) GetRideInProgress()
        {
            var ride = new Faker<Ride>()
                .RuleFor(x => x.Id, f => Guid.NewGuid())
                .RuleFor(x => x.Status, _ => RideStatus.InProgress)
                .Generate();

            var mockRepo = new Mock<IRideRepository>();
            mockRepo.Setup(x => x.GetByIdAsync(ride.Id)).ReturnsAsync(ride);

            var mockUow = new Mock<IUnitOfWork>();
            mockUow.Setup(x => x.RideRepository).Returns(mockRepo.Object);

            return (mockUow.Object, ride);
        }

    }
}
