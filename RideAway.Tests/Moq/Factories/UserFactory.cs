using Bogus;
using RideAway.Application.DTOs;
using RideAway.Domain.Entities;
using RideAway.Domain.Entities.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideAway.Tests.Moq.Factories
{
    public static class UserFactory
    {

        public static List<User> GenerateUsers(int count)
        {
            var faker = new Faker<User>()
                .RuleFor(x => x.Id, f => f.Random.Guid())
                .RuleFor(x => x.Name, f => f.Name.FullName())
                .RuleFor(x => x.Email, f => f.Internet.Email())
                .RuleFor(x => x.PhoneNumber, f => f.Phone.PhoneNumber())
                .RuleFor(x => x.Role, f => f.PickRandom<UserRole>());

            return faker.Generate(count);
        }

        public static User GenerateSingleUser(Guid userId)
        {
            return new Faker<User>()
                .RuleFor(x => x.Id, f => f.Random.Guid())
                .RuleFor(x => x.Name, f => f.Name.FullName())
                .RuleFor(x => x.Email, f => f.Internet.Email())
                .RuleFor(x => x.PhoneNumber, f => f.Phone.PhoneNumber())
                .RuleFor(x => x.Role, f => f.PickRandom<UserRole>());

        }

        public static User GenerateDriver()
        {
            var faker = new Faker<User>()
                .RuleFor(x => x.Id, f => f.Random.Guid())
                .RuleFor(x => x.Name, f => f.Name.FullName())
                .RuleFor(x => x.Email, f => f.Internet.Email())
                .RuleFor(x => x.PhoneNumber, f => f.Phone.PhoneNumber())
                .RuleFor(x => x.Role, f => f.PickRandom<UserRole>())
                .RuleFor(x => x.Vehicle, f => new Vehicle
                {
                    DriverId = f.Random.Guid(),
                    PlateNumber = f.Vehicle.Manufacturer(),
                    Model = f.Vehicle.Model(),
                    Category = f.Random.Enum<RideCategory>()
                })
                .RuleFor(x => x.CurrentLocation, f => f.Address.ToString());

            return faker.Generate();
        }

        public static User GenerateSingleDriver(Guid userId)
        {
            return new Faker<User>()
                .RuleFor(x => x.Id, f => f.Random.Guid())
                .RuleFor(x => x.Name, f => f.Name.FullName())
                .RuleFor(x => x.Email, f => f.Internet.Email())
                .RuleFor(x => x.PhoneNumber, f => f.Phone.PhoneNumber())
                .RuleFor(x => x.Role, f => f.PickRandom<UserRole>())
                .RuleFor(x => x.Vehicle, f => new Vehicle
                {
                    DriverId = f.Random.Guid(),
                    PlateNumber = f.Vehicle.Manufacturer(),
                    Model = f.Vehicle.Model(),
                    Category = f.Random.Enum<RideCategory>()
                })
                .RuleFor(x =>x.CurrentLocation, f =>f.Address.ToString());

        }

        public static CreateUserDTO GenerateUserDTO(UserRole role)
        {
            var faker = new Faker();
            return new CreateUserDTO
            {
                Name = faker.Name.FirstName(),
                Role = role

            };
        }
    }
}
