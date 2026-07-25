using System.Net.Http.Headers;
using System.Text;
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
        IOptions<PaymentSettings> settings,
        ILogger<PayPhonePaymentGateway> logger)
    {
        _http = http;
        _settings = settings.Value.PayPhone;
        _logger = logger;
    }

    public async Task<PaymentStartResult> CreatePaymentAsync(PaymentRequest request)
    {
        try
        {
            var transactionId = Guid.NewGuid().ToString("N");

            int amountInCents = (int)Math.Round(request.Monto * 100, MidpointRounding.AwayFromZero);

            var payment = new
            {
                id = transactionId,
                amount = amountInCents,
                amountWithTax = amountInCents,
                amountWithoutTax = 0,
                tax = 0,
                service = 0,
                tip = 0,
                clientTransactionId = request.VentaId.ToString(),
                storeId = _settings.ClientId,
                reference = request.Descripcion,
                returnUrl = request.ReturnUrl,
                cancelUrl = request.CancelUrl
            };

            var json = JsonSerializer.Serialize(payment);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.ClientSecret);

            var response = await _http.PostAsync($"{_settings.BaseUrl}/api/Sale", content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayPhone CreatePayment error {Status}: {Body}", response.StatusCode, body);
                return new PaymentStartResult { MensajeError = $"Error de PayPhone: {response.StatusCode}" };
            }

            var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var payWithPayPhone = root.TryGetProperty("payWithPayPhone", out var pwpp)
                ? pwpp.GetString() ?? ""
                : "";

            _logger.LogInformation("PayPhone pago creado: {TransactionId}", transactionId);

            return new PaymentStartResult
            {
                Exito = true,
                TokenPago = transactionId,
                UrlAprobacion = payWithPayPhone
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear pago en PayPhone");
            return new PaymentStartResult { MensajeError = ex.Message };
        }
    }

    public async Task<PaymentVerificationResult> VerifyPaymentAsync(string transactionId)
    {
        try
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.ClientSecret);

            var response = await _http.GetAsync($"{_settings.BaseUrl}/api/Sale/{transactionId}");
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayPhone Verify error {Status}: {Body}", response.StatusCode, body);
                return new PaymentVerificationResult { MensajeError = $"Error: {response.StatusCode}" };
            }

            var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var transactionStatus = root.TryGetProperty("transactionStatus", out var ts)
                ? ts.GetString() ?? "Unknown"
                : "Unknown";

            var aprobado = transactionStatus == "Authorized" || transactionStatus == "Approved";

            decimal montoConfirmado = 0;
            if (root.TryGetProperty("amount", out var amountElement))
            {
                montoConfirmado = amountElement.GetDecimal();
            }

            return new PaymentVerificationResult
            {
                Exito = true,
                Aprobado = aprobado,
                Estado = transactionStatus,
                MontoConfirmado = montoConfirmado
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar pago en PayPhone");
            return new PaymentVerificationResult { MensajeError = ex.Message };
        }
    }

    public async Task<PaymentCancellationResult> CancelPaymentAsync(string transactionId)
    {
        try
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.ClientSecret);

            var response = await _http.PostAsync(
                $"{_settings.BaseUrl}/api/Sale/{transactionId}/cancel", null);

            return new PaymentCancellationResult
            {
                Exito = response.IsSuccessStatusCode,
                Estado = "Cancelled",
                MensajeError = response.IsSuccessStatusCode ? null : $"Error: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cancelar pago en PayPhone");
            return new PaymentCancellationResult { MensajeError = ex.Message };
        }
    }
}
