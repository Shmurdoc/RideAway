using RideAway.Domain.Entities.Enum;
using RideAway.Domain.Value_Object;

namespace RideAway.Application.DTOs
{
    public class PaymentResultDTO
    {
        public bool IsSuccessful { get; set; }
        public string TransactionReference { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string? FailureReason { get; set; }
    }

    public class CreatePaymentRequestDTO : PaymentResultDTO
    {
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public Guid UserId { get; set; }
        public Guid RideId { get; set; }
        public PaymentStatus Status { get; set; }
    }

    public class PaymentDTO : CreatePaymentRequestDTO
    {
        
    }
}
