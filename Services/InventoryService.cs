using Microsoft.EntityFrameworkCore;
using HuellitasFelices.Data;
using HuellitasFelices.Models;

namespace HuellitasFelices.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly AppDbContext _context;
        private readonly IAuditService _auditService;
        private readonly ILogger<InventoryService> _logger;
        
        public InventoryService(AppDbContext context, IAuditService auditService, ILogger<InventoryService> logger)
        {
            _context = context;
            _auditService = auditService;
            _logger = logger;
        }
        
        public async Task<Inventario?> GetStockAsync(int productoId)
            => await _context.Inventarios.FirstOrDefaultAsync(i => i.ProductoId == productoId);
        
        public async Task<int> GetTotalStockAsync(int productoId)
            => await _context.Inventarios.Where(i => i.ProductoId == productoId).SumAsync(i => i.StockActual);
        
        public async Task<MovimientoInventario> RegistrarCompraAsync(int productoId, int cantidad, decimal precioUnitario, string? usuarioId, string? observacion)
        {
            var inventario = await GetStockAsync(productoId);
            var stockAnterior = inventario?.StockActual ?? 0;
            
            if (inventario == null)
            {
                inventario = new Inventario { ProductoId = productoId, StockActual = 0 };
                _context.Inventarios.Add(inventario);
            }
            
            inventario.StockActual += cantidad;
            inventario.FechaActualizacion = DateTime.UtcNow;
            
            var movimiento = new MovimientoInventario
            {
                TipoMovimiento = "Compra",
                Cantidad = cantidad,
                StockAnterior = stockAnterior,
                StockPosterior = inventario.StockActual,
                Observacion = observacion,
                ProductoId = productoId,
                UsuarioId = usuarioId,
                FechaMovimiento = DateTime.UtcNow
            };
            _context.MovimientosInventario.Add(movimiento);
            
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("InventarioCompra", "Producto", productoId, usuarioId: usuarioId,
                valorNuevo: $"Compra +{cantidad} unidades. Stock: {stockAnterior} → {inventario.StockActual}");
            
            _logger.LogInformation("[InventoryService] Compra registrada: Producto={ProductoId}, Cantidad={Cantidad}, Stock={Stock}",
                productoId, cantidad, inventario.StockActual);
            
            return movimiento;
        }
        
        public async Task<MovimientoInventario?> RegistrarVentaAsync(int productoId, int cantidad, string? usuarioId, string? referencia, string? observacion)
        {
            var inventario = await GetStockAsync(productoId);
            if (inventario == null || inventario.StockActual < cantidad)
            {
                _logger.LogWarning("[InventoryService] Stock insuficiente: Producto={ProductoId}, Stock={Stock}, Solicitado={Solicitado}",
                    productoId, inventario?.StockActual ?? 0, cantidad);
                return null;
            }
            
            var stockAnterior = inventario.StockActual;
            inventario.StockActual -= cantidad;
            inventario.FechaActualizacion = DateTime.UtcNow;
            
            var movimiento = new MovimientoInventario
            {
                TipoMovimiento = "Venta",
                Cantidad = cantidad,
                StockAnterior = stockAnterior,
                StockPosterior = inventario.StockActual,
                Referencia = referencia,
                Observacion = observacion,
                ProductoId = productoId,
                UsuarioId = usuarioId,
                FechaMovimiento = DateTime.UtcNow
            };
            _context.MovimientosInventario.Add(movimiento);
            
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("InventarioVenta", "Producto", productoId, usuarioId: usuarioId,
                valorAnterior: $"Stock: {stockAnterior}", valorNuevo: $"Venta -{cantidad} unidades. Stock: {inventario.StockActual}");
            
            return movimiento;
        }
        
        public async Task<MovimientoInventario?> AjustarAsync(int productoId, int nuevoStock, string motivo, string? usuarioId)
        {
            var inventario = await GetStockAsync(productoId);
            if (inventario == null)
            {
                inventario = new Inventario { ProductoId = productoId, StockActual = 0 };
                _context.Inventarios.Add(inventario);
            }
            
            var stockAnterior = inventario.StockActual;
            inventario.StockActual = nuevoStock;
            inventario.FechaActualizacion = DateTime.UtcNow;
            
            var movimiento = new MovimientoInventario
            {
                TipoMovimiento = "Ajuste",
                Cantidad = Math.Abs(nuevoStock - stockAnterior),
                StockAnterior = stockAnterior,
                StockPosterior = nuevoStock,
                Observacion = $"Ajuste: {motivo}",
                ProductoId = productoId,
                UsuarioId = usuarioId,
                FechaMovimiento = DateTime.UtcNow
            };
            _context.MovimientosInventario.Add(movimiento);
            
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("InventarioAjuste", "Producto", productoId, usuarioId: usuarioId,
                valorAnterior: $"Stock: {stockAnterior}", valorNuevo: $"Stock: {nuevoStock}. Motivo: {motivo}");
            
            return movimiento;
        }
        
        public async Task<List<MovimientoInventario>> GetMovimientosAsync(
            int? productoId = null, DateTime? desde = null, DateTime? hasta = null,
            int pagina = 1, int tamanioPagina = 20)
        {
            var query = _context.MovimientosInventario
                .AsNoTracking()
                .Include(m => m.Producto)
                .OrderByDescending(m => m.FechaMovimiento)
                .AsQueryable();
            
            if (productoId.HasValue) query = query.Where(m => m.ProductoId == productoId.Value);
            if (desde.HasValue) query = query.Where(m => m.FechaMovimiento >= desde.Value);
            if (hasta.HasValue) query = query.Where(m => m.FechaMovimiento <= hasta.Value);
            
            return await query.Skip((pagina - 1) * tamanioPagina).Take(tamanioPagina).ToListAsync();
        }
        
        public async Task<int> GetTotalMovimientosAsync(
            int? productoId = null, DateTime? desde = null, DateTime? hasta = null)
        {
            var query = _context.MovimientosInventario.AsNoTracking().AsQueryable();
            
            if (productoId.HasValue) query = query.Where(m => m.ProductoId == productoId.Value);
            if (desde.HasValue) query = query.Where(m => m.FechaMovimiento >= desde.Value);
            if (hasta.HasValue) query = query.Where(m => m.FechaMovimiento <= hasta.Value);
            
            return await query.CountAsync();
        }
    }
}
