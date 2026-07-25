using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HuellitasFelices.Settings;
using Microsoft.Extensions.Options;

namespace HuellitasFelices.Services.PaymentGateway;

public class PayPhonePaymentGateway : IPaymentGateway
{
    public string ProviderName => "PayPhone";

    private readonly HttpClient _http;
    private readonly PayPhoneSettings _settings;
    private readonly ILogger<PayPhonePaymentGateway> _logger;

    public PayPhonePaymentGateway(
        HttpClient http,
        IOptions<PayPhoneSettings> options,
        ILogger<PayPhonePaymentGateway> logger)
    {
        _http = http;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<PaymentStartResult> CreatePaymentAsync(PaymentRequest request)
    {
        try
        {
            string clientTransactionId = DateTime.Now.ToString("yyMMddHHmmssfff")[..15];
            int amountInCents = (int)Math.Round(request.Monto * 100, MidpointRounding.AwayFromZero);

            var payload = new
            {
                amount = amountInCents,
                amountWithoutTax = amountInCents,
                amountWithTax = 0,
                tax = 0,
                service = 0,
                tip = 0,
                currency = "USD",
                reference = request.Descripcion,
                clientTransactionId = clientTransactionId,
                additionalData = request.Descripcion,
                oneTime = true,
                expireIn = 0,
                isAmountEditable = false
            };

            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                "https://pay.payphonetodoesposible.com/api/Links");

            httpRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.Token);

            httpRequest.Content = JsonContent.Create(payload);

            var response = await _http.SendAsync(httpRequest);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayPhone API Links error {Status}: {Body}", response.StatusCode, content);
                return new PaymentStartResult { MensajeError = $"PayPhone respondió con error: {content}" };
            }

            var link = content.Trim('"');

            _logger.LogInformation("PayPhone link creado: {ClientTransactionId}", clientTransactionId);

            return new PaymentStartResult
            {
                Exito = true,
                TokenPago = clientTransactionId,
                UrlAprobacion = link
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear link de pago en PayPhone");
            return new PaymentStartResult { MensajeError = ex.Message };
        }
    }

    public Task<PaymentVerificationResult> VerifyPaymentAsync(string transactionId)
    {
        return Task.FromResult(new PaymentVerificationResult
        {
            Exito = true,
            Aprobado = true,
            Estado = "Approved",
            MontoConfirmado = 0
        });
    }

    public Task<PaymentCancellationResult> CancelPaymentAsync(string transactionId)
    {
        return Task.FromResult(new PaymentCancellationResult
        {
            Exito = true,
            Estado = "Cancelled"
        });
    }
}
