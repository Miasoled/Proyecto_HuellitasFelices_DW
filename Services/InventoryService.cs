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
        private const int MaxRetries = 3;

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
            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var inventario = await _context.Inventarios.FirstOrDefaultAsync(i => i.ProductoId == productoId);
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
                    await transaction.CommitAsync();

                    await _auditService.LogAsync("InventarioCompra", "Producto", productoId, usuarioId: usuarioId,
                        valorNuevo: $"Compra +{cantidad} unidades. Stock: {stockAnterior} → {inventario.StockActual}");

                    _logger.LogInformation("[InventoryService] Compra registrada: Producto={ProductoId}, Cantidad={Cantidad}, Stock={Stock}",
                        productoId, cantidad, inventario.StockActual);

                    return movimiento;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning(ex, "Conflicto de concurrencia al registrar compra. Intento {Intento}/{MaxRetries}", attempt + 1, MaxRetries);
                    foreach (var entry in _context.ChangeTracker.Entries())
                    {
                        if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                            entry.State = EntityState.Unchanged;
                    }
                    if (attempt == MaxRetries - 1) throw;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            throw new InvalidOperationException("No se pudo registrar la compra tras múltiples intentos.");
        }

        public async Task<MovimientoInventario?> RegistrarVentaAsync(int productoId, int cantidad, string? usuarioId, string? referencia, string? observacion)
        {
            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var inventario = await _context.Inventarios.FirstOrDefaultAsync(i => i.ProductoId == productoId);
                    if (inventario == null || inventario.StockActual < cantidad)
                    {
                        await transaction.RollbackAsync();
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
                    await transaction.CommitAsync();

                    await _auditService.LogAsync("InventarioVenta", "Producto", productoId, usuarioId: usuarioId,
                        valorAnterior: $"Stock: {stockAnterior}", valorNuevo: $"Venta -{cantidad} unidades. Stock: {inventario.StockActual}");

                    return movimiento;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning(ex, "Conflicto de concurrencia al registrar venta. Intento {Intento}/{MaxRetries}", attempt + 1, MaxRetries);
                    foreach (var entry in _context.ChangeTracker.Entries())
                    {
                        if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                            entry.State = EntityState.Unchanged;
                    }
                    if (attempt == MaxRetries - 1) throw;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            return null;
        }

        public async Task<MovimientoInventario?> AjustarAsync(int productoId, int nuevoStock, string motivo, string? usuarioId)
        {
            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var inventario = await _context.Inventarios.FirstOrDefaultAsync(i => i.ProductoId == productoId);
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
                    await transaction.CommitAsync();

                    await _auditService.LogAsync("InventarioAjuste", "Producto", productoId, usuarioId: usuarioId,
                        valorAnterior: $"Stock: {stockAnterior}", valorNuevo: $"Stock: {nuevoStock}. Motivo: {motivo}");

                    return movimiento;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning(ex, "Conflicto de concurrencia al ajustar inventario. Intento {Intento}/{MaxRetries}", attempt + 1, MaxRetries);
                    foreach (var entry in _context.ChangeTracker.Entries())
                    {
                        if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                            entry.State = EntityState.Unchanged;
                    }
                    if (attempt == MaxRetries - 1) throw;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            return null;
        }

        public async Task<List<MovimientoInventario>> ReservarStockParaVentaAsync(
            int ventaId, List<(int ProductoId, int Cantidad)> items, string? usuarioId)
        {
            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var movimientos = new List<MovimientoInventario>();

                    foreach (var (productoId, cantidad) in items)
                    {
                        var inventario = await _context.Inventarios
                            .FirstOrDefaultAsync(i => i.ProductoId == productoId);

                        if (inventario == null || inventario.StockActual < cantidad)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogWarning(
                                "[InventoryService] Stock insuficiente para reserva: Producto={ProductoId}, Stock={Stock}, Solicitado={Solicitado}",
                                productoId, inventario?.StockActual ?? 0, cantidad);
                            return null!;
                        }

                        var stockAnterior = inventario.StockActual;
                        inventario.StockActual -= cantidad;
                        inventario.FechaActualizacion = DateTime.UtcNow;

                        var movimiento = new MovimientoInventario
                        {
                            TipoMovimiento = "Reserva",
                            Cantidad = cantidad,
                            StockAnterior = stockAnterior,
                            StockPosterior = inventario.StockActual,
                            Referencia = $"Venta-{ventaId}",
                            Observacion = $"Reserva para venta #{ventaId}",
                            ProductoId = productoId,
                            UsuarioId = usuarioId,
                            FechaMovimiento = DateTime.UtcNow
                        };
                        _context.MovimientosInventario.Add(movimiento);
                        movimientos.Add(movimiento);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    foreach (var mov in movimientos)
                    {
                        await _auditService.LogAsync("InventarioReserva", "Producto", mov.ProductoId,
                            usuarioId: usuarioId,
                            valorNuevo: $"Reserva -{mov.Cantidad} unidades. Stock: {mov.StockAnterior} → {mov.StockPosterior}");
                    }

                    _logger.LogInformation("[InventoryService] Reserva creada para venta {VentaId}: {CantidadProductos} productos",
                        ventaId, items.Count);

                    return movimientos;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning(ex, "Conflicto de concurrencia al reservar stock. Intento {Intento}/{MaxRetries}", attempt + 1, MaxRetries);
                    foreach (var entry in _context.ChangeTracker.Entries())
                    {
                        if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                            entry.State = EntityState.Unchanged;
                    }
                    if (attempt == MaxRetries - 1) throw;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            return null!;
        }

        public async Task RevertirReservaAsync(int ventaId, string? usuarioId, string motivo)
        {
            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var reservas = await _context.MovimientosInventario
                        .Where(m => m.TipoMovimiento == "Reserva" && m.Referencia == $"Venta-{ventaId}")
                        .ToListAsync();

                    if (reservas.Count == 0)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogInformation("[InventoryService] No hay reservas que revertir para venta {VentaId}", ventaId);
                        return;
                    }

                    foreach (var reserva in reservas)
                    {
                        var inventario = await _context.Inventarios
                            .FirstOrDefaultAsync(i => i.ProductoId == reserva.ProductoId);

                        if (inventario == null) continue;

                        var stockAnterior = inventario.StockActual;
                        inventario.StockActual += reserva.Cantidad;
                        inventario.FechaActualizacion = DateTime.UtcNow;

                        var movimientoRevertido = new MovimientoInventario
                        {
                            TipoMovimiento = "Revertido",
                            Cantidad = reserva.Cantidad,
                            StockAnterior = stockAnterior,
                            StockPosterior = inventario.StockActual,
                            Referencia = $"Venta-{ventaId}",
                            Observacion = $"Reversión de reserva por: {motivo}",
                            ProductoId = reserva.ProductoId,
                            UsuarioId = usuarioId,
                            FechaMovimiento = DateTime.UtcNow
                        };
                        _context.MovimientosInventario.Add(movimientoRevertido);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("[InventoryService] Reserva revertida para venta {VentaId}: {Cantidad} productos restaurados",
                        ventaId, reservas.Count);

                    foreach (var reserva in reservas)
                    {
                        await _auditService.LogAsync("InventarioRevertido", "Producto", reserva.ProductoId,
                            usuarioId: usuarioId,
                            valorNuevo: $"Reversión +{reserva.Cantidad} unidades por: {motivo}");
                    }
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning(ex, "Conflicto de concurrencia al revertir reserva. Intento {Intento}/{MaxRetries}", attempt + 1, MaxRetries);
                    foreach (var entry in _context.ChangeTracker.Entries())
                    {
                        if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                            entry.State = EntityState.Unchanged;
                    }
                    if (attempt == MaxRetries - 1) throw;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
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