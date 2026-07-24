namespace HuellitasFelices.Services.PaymentGateway;

public interface IPaymentGateway
{
    string ProviderName { get; }
    Task<PaymentStartResult> CreatePaymentAsync(PaymentRequest request);
    Task<PaymentVerificationResult> VerifyPaymentAsync(string transactionId);
    Task<PaymentCancellationResult> CancelPaymentAsync(string transactionId);
}
