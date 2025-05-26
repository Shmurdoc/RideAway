using Bogus;
using RideAway.Application.DTOs;
using RideAway.Application.Features.Payments.Commands;
using RideAway.Domain.Entities;
using RideAway.Domain.Entities.Enum;
using System;

namespace RideAway.Tests.Moq
{
    public static class MockPaymentRepository
    {
        public static Ride GetFakeRide()
        {
            return new Faker<Ride>()
                .RuleFor(r => r.RiderId, f => f.Random.Guid())
                .Generate();
        }


        public static ProcessPaymentCommand GetFakeCommand(Guid? rideId = null)
        {
            var faker = new Faker<ProcessPaymentCommand>()
                .CustomInstantiator(f => new ProcessPaymentCommand(
                    rideId ?? Guid.NewGuid(),
                    f.Random.Guid(),
                    f.Finance.Amount(50, 300),
                    f.PickRandom<PaymentMethod>()
                ));

            return faker.Generate();
        }


        public static PaymentResultDTO GetSuccessfulPaymentResult()
        {
            return new Faker<PaymentResultDTO>()
                .RuleFor(p => p.IsSuccessful, _ => true)
                .RuleFor(p => p.TransactionReference, f => f.Finance.TransactionType())
                .RuleFor(p => p.PaymentDate, _ => DateTime.UtcNow)
                .Generate();
        }

        public static PaymentResultDTO GetFailedPaymentResult()
        {
            return new Faker<PaymentResultDTO>()
                .RuleFor(p => p.IsSuccessful, _ => false)
                .RuleFor(p => p.TransactionReference, f => f.Finance.TransactionType())
                .RuleFor(p => p.PaymentDate, _ => DateTime.UtcNow)
                .Generate();
        }
    }
}
