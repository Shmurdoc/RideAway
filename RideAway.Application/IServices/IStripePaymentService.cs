namespace RideAway.Application.IServices
{
    public interface IStripePaymentService
    {
        Task<PaymentResult> CreatePaymentSession(decimal amount, string currency);
    }

    public class PaymentResult
    {
        public string Reference { get; set; } = null!;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

