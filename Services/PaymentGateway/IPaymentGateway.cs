using HuellitasFelices.Models;

namespace HuellitasFelices.Services.PaymentGateway;

public interface IPaymentGateway
{
    string ProviderName { get; }
    Task<PaymentStartResult> CreatePaymentAsync(PaymentRequest request);
    Task<PaymentVerificationResult> VerifyPaymentAsync(Pago pago);
    Task<PaymentCancellationResult> CancelPaymentAsync(string transactionId);
}
