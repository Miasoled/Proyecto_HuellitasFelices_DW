using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HuellitasFelices.Models;
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

            var baseUrl = _settings.BaseUrl.TrimEnd('/');
            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                $"{baseUrl}/api/Links");

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

    public async Task<PaymentVerificationResult> VerifyPaymentAsync(Pago pago)
    {
        try
        {
            var payPhoneId = pago.IdentificadorExterno;
            var clientTransactionId = pago.TokenPasarela;

            if (string.IsNullOrEmpty(payPhoneId))
            {
                _logger.LogWarning("PayPhone Verify: IdentificadorExterno vacío para pago {PagoId}", pago.Id);
                return new PaymentVerificationResult
                {
                    Exito = false,
                    Aprobado = false,
                    Estado = "PENDING",
                    MensajeError = "PayPhone no devolvió el identificador de la transacción."
                };
            }

            if (!int.TryParse(payPhoneId, out var payPhoneTransactionId))
            {
                _logger.LogWarning("PayPhone Verify: identificador externo inválido para pago {PagoId}", pago.Id);
                return new PaymentVerificationResult
                {
                    Exito = false,
                    Aprobado = false,
                    Estado = "PENDING",
                    MensajeError = "El identificador de transacción de PayPhone no es válido."
                };
            }

            var confirmRequest = new
            {
                id = payPhoneTransactionId,
                clientTxId = clientTransactionId ?? ""
            };

            var baseUrl = _settings.BaseUrl.TrimEnd('/');
            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                $"{baseUrl}/api/button/V2/Confirm");

            httpRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.Token);

            httpRequest.Content = JsonContent.Create(confirmRequest);

            var response = await _http.SendAsync(httpRequest);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("PayPhone Confirm returned {Status}: {Body}", response.StatusCode, body);
                return new PaymentVerificationResult
                {
                    Exito = false,
                    Aprobado = false,
                    Estado = "ERROR",
                    MensajeError = $"PayPhone respondió con error: {(int)response.StatusCode}."
                };
            }

            var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var transactionStatus = root.TryGetProperty("transactionStatus", out var ts)
                ? ts.GetString() ?? "Unknown"
                : "Unknown";

            var aprobado = transactionStatus == "Approved" || transactionStatus == "Authorized";

            decimal montoConfirmado = 0;
            if (root.TryGetProperty("amount", out var amountElement))
            {
                // PayPhone trabaja con centavos; el resto de la aplicación usa USD.
                montoConfirmado = amountElement.GetDecimal() / 100m;
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
            return new PaymentVerificationResult
            {
                Exito = false,
                Aprobado = false,
                Estado = "ERROR",
                MensajeError = "No fue posible verificar el pago con PayPhone."
            };
        }
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
