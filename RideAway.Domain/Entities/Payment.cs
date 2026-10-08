using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RideAway.Domain.Entities.Enum;
using RideAway.Domain.Exceptions;
using RideAway.Domain.Value_Object;

namespace RideAway.Domain.Entities
{
    public class Payment : BaseEntity
    {
        /// <summary>The ride this payment settles. One settlement per ride, enforced by unique index.</summary>
        public Guid RideId { get; set; }

        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public PaymentStatus Status { get; set; }
        public bool IsSuccessful { get; set; }
        public string? TransactionReference { get; set; } // options for Stripe/Card
        public string? FailureReason { get; set; }

        /// <summary>Creates a pending payment. Settlement happens through confirmation paths.</summary>
        public static Payment CreatePending(Guid rideId, Guid userId, decimal amount, PaymentMethod method, string? transactionReference = null)
        {
            if (amount <= 0)
                throw new ArgumentException("Payment amount must be greater than zero.", nameof(amount));

            return new Payment
            {
                RideId = rideId,
                UserId = userId,
                Amount = amount,
                Method = method,
                Status = PaymentStatus.Pending,
                IsSuccessful = false,
                PaymentDate = DateTime.UtcNow,
                TransactionReference = transactionReference
            };
        }

        /// <summary>Marks the payment successful. Settles once; rejects replays.</summary>
        public void MarkAsCompleted(string? transactionReference = null)
        {
            if (Status == PaymentStatus.Completed)
                throw new PaymentProcessingException("This payment has already been completed.");

            Status = PaymentStatus.Completed;
            IsSuccessful = true;
            FailureReason = null;

            if (!string.IsNullOrWhiteSpace(transactionReference))
                TransactionReference = transactionReference;
        }

        public void MarkAsFailed(string reason)
        {
            Status = PaymentStatus.Failed;
            IsSuccessful = false;
            FailureReason = reason;
        }
    }

}
