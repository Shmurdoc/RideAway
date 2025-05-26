using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RideAway.Domain.Entities.Enum;
using RideAway.Domain.Value_Object;

namespace RideAway.Domain.Entities
{
    public class Payment : BaseEntity
    {
        public Guid PaymentId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public PaymentStatus Status { get; set; }
        public bool IsSuccessful { get; set; }
        public string? TransactionReference { get; set; } // options for Stripe/Card
        public string? FailureReason { get; set; }
    }

}
