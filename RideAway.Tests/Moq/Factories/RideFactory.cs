using Bogus;
using RideAway.Application.DTOs;
using RideAway.Domain.Entities;
using RideAway.Domain.Entities.Enum;
using RideAway.Domain.Value_Object;
using System;
using System.Collections.Generic;
using RideAlias = RideAway.Domain.Entities.Ride;

namespace RideAway.Tests.Moq.Factories
{
    public static class RideFactory
    {
        public static List<Ride> GenerateRides(int count)
        {
            var faker = new Faker<Ride>()
                .RuleFor(x => x.Id, f => f.Random.Guid())
                .RuleFor(x => x.DriverId, f => f.Random.Guid())
                .RuleFor(x => x.PickupLocation, f => f.Address.FullAddress())
                .RuleFor(x => x.Destination, f => f.Address.FullAddress())
                .RuleFor(x => x.Fare, f => f.Random.Decimal(10, 200))
                .RuleFor(x => x.RiderCategory, f => f.PickRandom<RideCategory>())
                .RuleFor(x => x.Status, f => RideStatus.Requested);

            return faker.Generate(count);
        }

        public static Ride GenerateSingleRide(Guid rideId, Guid driverId)
        {
            return new Faker<Ride>()
                .RuleFor(x => x.Id, _ => rideId)
                .RuleFor(x => x.DriverId, _ => driverId)
                .RuleFor(x => x.PickupLocation, f => f.Address.FullAddress())
                .RuleFor(x => x.Destination, f => f.Address.FullAddress())
                .RuleFor(x => x.Fare, f => f.Random.Decimal(10, 200))
                .RuleFor(x => x.RiderCategory, f => f.PickRandom<RideCategory>())
                .RuleFor(x => x.Driver, f => UserFactory.GenerateSingleDriver(driverId))
                .RuleFor(x => x.Status, _ => RideStatus.InProgress)
                .Generate();
        }

        public static CreateRideRequestDTO GenerateRideRequestDTO(bool withDriver = true)
        {
            var faker = new Faker();
            return new CreateRideRequestDTO
            {
                PickupLocation = faker.Address.FullAddress(),
                Destination = faker.Address.FullAddress(),
                DriverId = withDriver ? faker.Random.Guid() : Guid.Empty,
                RideCategory = faker.PickRandom<RideCategory>()
            };
        }

        public static RideAlias GenerateRideAlias()
        {
            var faker = new Faker<RideAlias>()
                .RuleFor(x => x.DriverId, f => f.Random.Guid())
                .RuleFor(x => x.PickupLocation, f => f.Address.FullAddress())
                .RuleFor(x => x.Destination, f => f.Address.FullAddress())
                .RuleFor(x => x.Fare, f => f.Random.Decimal(10, 200))
                .RuleFor(x => x.RiderCategory, f => f.PickRandom<RideCategory>());
            return faker.Generate();
        }
    }
}
