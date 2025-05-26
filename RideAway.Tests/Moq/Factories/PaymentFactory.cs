using Bogus;
using RideAway.Application.DTOs;
using RideAway.Application.Features.Payments.Commands;
using RideAway.Domain.Entities;
using RideAway.Domain.Entities.Enum;
using RideAway.Domain.Value_Object;
using System;

namespace RideAway.Tests.Moq.Factories;
public static class PaymentFactory
{
    public static ProcessPaymentCommand GetFakeCommand(Guid? rideId = null)
    {
        return new Faker<ProcessPaymentCommand>()
            .CustomInstantiator(f => new ProcessPaymentCommand(
                rideId ?? f.Random.Guid(),
                f.Random.Guid(),
                f.Finance.Amount(50, 300),
                f.PickRandom<PaymentMethod>()
            ))
            .Generate();
    }

    public static PaymentResultDTO GetSuccessfulPaymentResult()
    {
        return new Faker<PaymentResultDTO>()
            .RuleFor(p => p.IsSuccessful, _ => true)
            .RuleFor(p => p.TransactionReference, f => f.Random.AlphaNumeric(10))
            .RuleFor(p => p.PaymentDate, f => f.Date.Past(1))
            .Generate();
    }

    public static Ride GetFakeRide(Guid? rideId = null)
    {
        return new Faker<Ride>()
            .CustomInstantiator(f => new Ride(
                f.Address.StreetAddress(),
                f.Address.City(),
                f.Finance.Amount(10, 100)
            ))
            .RuleFor(r => r.Id, _ => rideId ?? Guid.NewGuid())
            .RuleFor(r => r.RiderId, f => f.Random.Guid())
            .RuleFor(r => r.DriverId, f => f.Random.Guid())
            .RuleFor(r => r.Status, _ => RideStatus.Requested)
            .Generate();
    }
}
