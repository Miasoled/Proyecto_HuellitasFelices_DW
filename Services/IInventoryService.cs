using HuellitasFelices.Models;

namespace HuellitasFelices.Services
{
    public interface IInventoryService
    {
        Task<Inventario?> GetStockAsync(int productoId);
        Task<int> GetTotalStockAsync(int productoId);
        Task<MovimientoInventario> RegistrarCompraAsync(int productoId, int cantidad, decimal precioUnitario, string? usuarioId, string? observacion);
        Task<MovimientoInventario?> RegistrarVentaAsync(int productoId, int cantidad, string? usuarioId, string? referencia, string? observacion);
        Task<MovimientoInventario?> AjustarAsync(int productoId, int nuevoStock, string motivo, string? usuarioId);
        Task<List<MovimientoInventario>> GetMovimientosAsync(int? productoId = null, DateTime? desde = null, DateTime? hasta = null, int pagina = 1, int tamanioPagina = 20);
        Task<int> GetTotalMovimientosAsync(int? productoId = null, DateTime? desde = null, DateTime? hasta = null);
    }
}
