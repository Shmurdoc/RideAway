namespace RideAway.Application.DTOs
{
    public class PaymentResultDTO
    {
        public bool IsSuccessful { get; set; }
        public string TransactionReference { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string? FailureReason { get; set; }
    }
}
