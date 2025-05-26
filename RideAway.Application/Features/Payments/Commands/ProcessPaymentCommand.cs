using MediatR;
using RideAway.Application.DTOs;
using RideAway.Domain.Entities.Enum;

namespace RideAway.Application.Features.Payments.Commands
{
    public record ProcessPaymentCommand(
          Guid RideId,
          Guid UserId,
          decimal Amount,
          PaymentMethod PaymentMethod
      ) : IRequest<PaymentResultDTO>;

}
