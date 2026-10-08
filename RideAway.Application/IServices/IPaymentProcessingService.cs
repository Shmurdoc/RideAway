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
        /// <summary>Creates the single pending payment record for a ride.</summary>
        Task<PaymentResultDTO> CreatePaymentAsync(Guid rideId, Guid userId, decimal amount, PaymentMethod method);

        /// <summary>Settles a payment from a signature-verified Stripe webhook event.</summary>
        Task ConfirmPaymentAsync(string transactionReference, decimal verifiedAmount, string currency);

        /// <summary>Settles a cash payment. Only the assigned driver may call this.</summary>
        Task ConfirmCashCollectionAsync(Guid rideId, Guid driverId);
    }
}
