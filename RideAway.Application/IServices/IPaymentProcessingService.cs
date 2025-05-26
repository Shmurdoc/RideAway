using RideAway.Application.DTOs;
using RideAway.Domain.Entities;
using RideAway.Domain.Entities.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideAway.Application.IServices
{
    public interface IPaymentProcessingService
    {
        Task<bool> ProcessPayment(Guid rideId);
        Task<PaymentResultDTO> ProcessPaymentAsync(Guid userId, decimal amount, PaymentMethod method);
    }
}
