using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using HuellitasFelices.Settings;
using Microsoft.Extensions.Options;

namespace HuellitasFelices.Services.PaymentGateway;

public class PayPalPaymentGateway : IPaymentGateway
{
    public string ProviderName => "PayPal";

    private readonly HttpClient _http;
    private readonly PayPalSettings _settings;
    private readonly ILogger<PayPalPaymentGateway> _logger;

    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public PayPalPaymentGateway(
        HttpClient http,
        IOptions<PaymentSettings> settings,
        ILogger<PayPalPaymentGateway> logger)
    {
        _http = http;
        _settings = settings.Value.PayPal;
        _logger = logger;
    }

    public async Task<PaymentStartResult> CreatePaymentAsync(PaymentRequest request)
    {
        try
        {
            var token = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
                return new PaymentStartResult { MensajeError = "No se pudo obtener token de acceso de PayPal" };

            var order = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new
                    {
                        amount = new
                        {
                            currency_code = request.Moneda,
                            value = request.Monto.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
                        },
                        description = request.Descripcion
                    }
                },
                application_context = new
                {
                    return_url = request.ReturnUrl,
                    cancel_url = request.CancelUrl,
                    brand_name = "Huellitas Felices",
                    locale = "en-US",
                    landing_page = "BILLING",
                    user_action = "PAY_NOW"
                }
            };

            var json = JsonSerializer.Serialize(order);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var req = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/v2/checkout/orders");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            req.Content = content;

            var response = await _http.SendAsync(req);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayPal CreateOrder error {Status}: {Body}", response.StatusCode, body);
                return new PaymentStartResult { MensajeError = $"Error de PayPal ({response.StatusCode}): {body}" };
            }

            var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var orderId = root.GetProperty("id").GetString() ?? "";
            var links = root.GetProperty("links");
            var approvalUrl = "";

            foreach (var link in links.EnumerateArray())
            {
                if (link.GetProperty("rel").GetString() == "approve")
                {
                    approvalUrl = link.GetProperty("href").GetString() ?? "";
                    break;
                }
            }

            _logger.LogInformation("PayPal order creada: {OrderId}", orderId);

            return new PaymentStartResult
            {
                Exito = true,
                TokenPago = orderId,
                UrlAprobacion = approvalUrl
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear order en PayPal");
            return new PaymentStartResult { MensajeError = ex.Message };
        }
    }

    public async Task<PaymentVerificationResult> VerifyPaymentAsync(string orderId)
    {
        try
        {
            var token = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
                return new PaymentVerificationResult { MensajeError = "No se pudo obtener token de acceso" };

            var req = new HttpRequestMessage(HttpMethod.Get, $"{_settings.BaseUrl}/v2/checkout/orders/{orderId}");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _http.SendAsync(req);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayPal GetOrder error {Status}: {Body}", response.StatusCode, body);
                return new PaymentVerificationResult { MensajeError = $"Error: {response.StatusCode}" };
            }

            var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var status = root.GetProperty("status").GetString() ?? "UNKNOWN";

            if (status == "APPROVED")
            {
                var captureResult = await CaptureOrderAsync(orderId, token);
                if (captureResult.Exito)
                {
                    status = "COMPLETED";
                    return new PaymentVerificationResult
                    {
                        Exito = true,
                        Aprobado = true,
                        Estado = status,
                        MontoConfirmado = captureResult.MontoConfirmado
                    };
                }

                return new PaymentVerificationResult
                {
                    Exito = true,
                    Aprobado = false,
                    Estado = "CAPTURE_FAILED",
                    MensajeError = captureResult.MensajeError
                };
            }

            var aprobado = status == "COMPLETED";

            decimal montoConfirmado = 0;
            if (root.TryGetProperty("purchase_units", out var units) && units.GetArrayLength() > 0)
            {
                var unit = units[0];
                if (unit.TryGetProperty("payments", out var payments) &&
                    payments.TryGetProperty("captures", out var captures) &&
                    captures.GetArrayLength() > 0)
                {
                    var cap = captures[0];
                    if (cap.TryGetProperty("amount", out var amount) &&
                        amount.TryGetProperty("value", out var val))
                    {
                        decimal.TryParse(val.GetString(), System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out montoConfirmado);
                    }
                }
            }

            return new PaymentVerificationResult
            {
                Exito = true,
                Aprobado = aprobado,
                Estado = status,
                MontoConfirmado = montoConfirmado
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar pago en PayPal");
            return new PaymentVerificationResult { MensajeError = ex.Message };
        }
    }

    public async Task<PaymentCancellationResult> CancelPaymentAsync(string orderId)
    {
        try
        {
            var token = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
                return new PaymentCancellationResult { MensajeError = "No se pudo obtener token de acceso" };

            var req = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/v2/checkout/orders/{orderId}/cancel");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _http.SendAsync(req);

            return new PaymentCancellationResult
            {
                Exito = response.IsSuccessStatusCode,
                Estado = "cancelled",
                MensajeError = response.IsSuccessStatusCode ? null : $"Error: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cancelar pago en PayPal");
            return new PaymentCancellationResult { MensajeError = ex.Message };
        }
    }

    private async Task<PaymentVerificationResult> CaptureOrderAsync(string orderId, string token)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/v2/checkout/orders/{orderId}/capture");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            req.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(req);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayPal Capture error {Status}: {Body}", response.StatusCode, body);
                return new PaymentVerificationResult { MensajeError = $"Capture failed: {response.StatusCode}: {body}" };
            }

            var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var status = root.GetProperty("status").GetString() ?? "UNKNOWN";

            decimal montoConfirmado = 0;
            if (root.TryGetProperty("purchase_units", out var units) && units.GetArrayLength() > 0)
            {
                var unit = units[0];
                if (unit.TryGetProperty("payments", out var payments) &&
                    payments.TryGetProperty("captures", out var captures) &&
                    captures.GetArrayLength() > 0)
                {
                    var cap = captures[0];
                    if (cap.TryGetProperty("amount", out var amount) &&
                        amount.TryGetProperty("value", out var val))
                    {
                        decimal.TryParse(val.GetString(), System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out montoConfirmado);
                    }
                }
            }

            _logger.LogInformation("PayPal order {OrderId} capturada. Status: {Status}", orderId, status);

            return new PaymentVerificationResult
            {
                Exito = true,
                Aprobado = status == "COMPLETED",
                Estado = status,
                MontoConfirmado = montoConfirmado
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al capturar order en PayPal");
            return new PaymentVerificationResult { MensajeError = ex.Message };
        }
    }

    private async Task<string?> GetAccessTokenAsync()
    {
        if (_accessToken != null && DateTime.UtcNow < _tokenExpiry)
            return _accessToken;

        try
        {
            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}"));

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/v1/oauth2/token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            request.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = await _http.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayPal token error {Status}: {Body}", response.StatusCode, body);
                return null;
            }

            var doc = JsonDocument.Parse(body);
            _accessToken = doc.RootElement.GetProperty("access_token").GetString();
            var expiresIn = doc.RootElement.GetProperty("expires_in").GetInt32();
            _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn - 60);

            return _accessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener token de PayPal");
            return null;
        }
    }
}
