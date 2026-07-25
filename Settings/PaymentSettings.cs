namespace HuellitasFelices.Settings;

public class PaymentSettings
{
    public PayPalSettings PayPal { get; set; } = new();
    public PayPhoneSettings PayPhone { get; set; } = new();
}

public class PayPalSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api-m.sandbox.paypal.com";
    public string ReturnUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}

public class PayPhoneSettings
{
    public string Token { get; set; } = string.Empty;
    public string StoreId { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://pay.payphonetodoesposible.com";
    public string ReturnUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}
