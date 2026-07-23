using HuellitasFelices.Models;

namespace HuellitasFelices.Services
{
    public interface IInventoryService
    {
        Task<Inventario?> GetStockAsync(int productoId, int sucursalId);
        Task<int> GetTotalStockAsync(int productoId);
        Task<MovimientoInventario> RegistrarCompraAsync(int productoId, int sucursalId, int cantidad, decimal precioUnitario, string? usuarioId, string? observacion);
        Task<MovimientoInventario?> RegistrarVentaAsync(int productoId, int sucursalId, int cantidad, string? usuarioId, string? referencia, string? observacion);
        Task<MovimientoInventario?> TransferirAsync(int productoId, int sucursalOrigenId, int sucursalDestinoId, int cantidad, string? usuarioId, string? observacion);
        Task<MovimientoInventario?> AjustarAsync(int productoId, int sucursalId, int nuevoStock, string motivo, string? usuarioId);
        Task<List<MovimientoInventario>> GetMovimientosAsync(int? productoId = null, int? sucursalId = null, DateTime? desde = null, DateTime? hasta = null, int pagina = 1, int tamanioPagina = 20);
        Task<int> GetTotalMovimientosAsync(int? productoId = null, int? sucursalId = null, DateTime? desde = null, DateTime? hasta = null);
    }
}
