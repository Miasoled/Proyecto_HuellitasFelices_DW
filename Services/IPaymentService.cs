using HuellitasFelices.Models;

namespace HuellitasFelices.Services;

public interface IPaymentService
{
    Task<Pago> CrearPagoAsync(int ventaId, decimal monto, string proveedor, string returnUrl, string cancelUrl);
    Task<Pago?> ConfirmarPagoAsync(int pagoId);
    Task<Pago?> CancelarPagoAsync(int pagoId);
    Task<Pago?> ObtenerPagoAsync(int pagoId);
    Task<List<Pago>> ObtenerPagosAsync(int pagina = 1, int tamanioPagina = 20,
        string? busqueda = null, string? estado = null, string? proveedor = null,
        DateTime? desde = null, DateTime? hasta = null);
    Task<int> TotalPagosAsync(string? busqueda = null, string? estado = null,
        string? proveedor = null, DateTime? desde = null, DateTime? hasta = null);
    Task<List<Pago>> ObtenerPagosPorVentaAsync(int ventaId);
}
