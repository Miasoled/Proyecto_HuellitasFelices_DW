using HuellitasFelices.Models;

namespace HuellitasFelices.Services
{
    public interface IInventoryService
    {
        Task<Inventario?> GetStockAsync(int productoId);
        Task<int> GetTotalStockAsync(int productoId);
        Task<MovimientoInventario> RegistrarCompraAsync(int compraId, int productoId, int cantidad, string? usuarioId, int? sucursalId);
        Task<MovimientoInventario?> RegistrarVentaAsync(int productoId, int cantidad, string? usuarioId, string? referencia, string? observacion);
        Task<MovimientoInventario?> AjustarAsync(int productoId, int? sucursalId, int nuevoStock, string? usuarioId, string? motivo);
        Task<bool> RegistrarDevolucionAsync(int productoId, int sucursalId, int cantidad, int? ventaId, string? usuarioId, string? motivo);
        Task<List<MovimientoInventario>> GetMovimientosAsync(int? productoId = null, DateTime? desde = null, DateTime? hasta = null, int pagina = 1, int tamanioPagina = 20);
        Task<int> GetTotalMovimientosAsync(int? productoId = null, DateTime? desde = null, DateTime? hasta = null);
        Task<List<MovimientoInventario>> ReservarStockParaVentaAsync(int ventaId, List<(int ProductoId, int Cantidad)> items, string? usuarioId);
        Task RevertirReservaAsync(int ventaId, string? usuarioId, string motivo);
        Task<bool> TransferirStockAsync(int productoId, int sucursalOrigenId, int sucursalDestinoId, int cantidad, string? usuarioId, string? observacion);
    }
}
