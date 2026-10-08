using MediatR;
using RideAway.Application.DTOs;
using RideAway.Domain.Entities.Enum;

namespace RideAway.Application.Features.Payments.Commands
{
    /// <summary>
    /// Starts payment for a ride. The amount is never supplied by the client: it is
    /// read from the ride's server-computed fare. <paramref name="RiderId"/> is the
    /// authenticated caller, and must own the ride.
    /// </summary>
    public record ProcessPaymentCommand(
          Guid RideId,
          Guid RiderId,
          PaymentMethod PaymentMethod
      ) : IRequest<PaymentResultDTO>;

}
