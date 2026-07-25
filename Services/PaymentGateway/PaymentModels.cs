namespace HuellitasFelices.Services.PaymentGateway;

public class PaymentRequest
{
    public decimal Monto { get; set; }
    public string Moneda { get; set; } = "USD";
    public string Descripcion { get; set; } = string.Empty;
    public int VentaId { get; set; }
    public string ReturnUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}

public class PaymentStartResult
{
    public bool Exito { get; set; }
    public string? TokenPago { get; set; }
    public string? UrlAprobacion { get; set; }
    public string? MensajeError { get; set; }
}

public class PaymentVerificationResult
{
    public bool Exito { get; set; }
    public bool Aprobado { get; set; }
    public string? Estado { get; set; }
    public string? MensajeError { get; set; }
    public decimal MontoConfirmado { get; set; }
}

public class PaymentCancellationResult
{
    public bool Exito { get; set; }
    public string? Estado { get; set; }
    public string? MensajeError { get; set; }
}
