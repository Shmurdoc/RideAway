using MediatR;
using RideAway.Application.DTOs;
using RideAway.Domain.Entities.Enum;

namespace RideAway.Application.Features.Payments.Commands
{
    /// <summary>Amount comes from the ride fare. RiderId is the caller from the token.</summary>
    public record ProcessPaymentCommand(
          Guid RideId,
          Guid RiderId,
          PaymentMethod PaymentMethod
      ) : IRequest<PaymentResultDTO>;

}
