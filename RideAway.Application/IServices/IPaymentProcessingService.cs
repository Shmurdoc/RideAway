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
        /// <summary>Creates the pending payment for a ride. One row, pending.</summary>
        Task<PaymentResultDTO> CreatePaymentAsync(Guid rideId, Guid userId, decimal amount, PaymentMethod method);

        /// <summary>Settles a payment from a verified Stripe webhook.</summary>
        Task ConfirmPaymentAsync(string transactionReference, decimal verifiedAmount, string currency);

        /// <summary>Settles cash. Assigned driver only.</summary>
        Task ConfirmCashCollectionAsync(Guid rideId, Guid driverId);
    }
}
