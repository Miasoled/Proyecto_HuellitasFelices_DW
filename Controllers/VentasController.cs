using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HuellitasFelices.Data;
using HuellitasFelices.Models;

namespace HuellitasFelices.Controllers
{
    [Authorize]
    public class VentasController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public VentasController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Ventas/Pagar?consultaId=5
        [HttpGet]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Pagar(int? consultaId)
        {
            if (consultaId == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            var dueno = await _context.Duenos
                .FirstOrDefaultAsync(d => d.Email != null && d.Email == user.Email && d.Activo);
            if (dueno == null) return Forbid();

            var consulta = await _context.Consultas
                .Include(c => c.Mascota)
                .Include(c => c.Veterinario)
                .Include(c => c.Medicamentos).ThenInclude(cm => cm.Producto)
                .Include(c => c.Venta)
                .FirstOrDefaultAsync(c => c.Id == consultaId && c.Activo);

            if (consulta == null) return NotFound();
            if (consulta.Mascota?.DuenoId != dueno.Id) return Forbid();

            if (consulta.Estado != "EnRevision")
                return RedirectToAction("MiPanel", "Account");

            if (consulta.Venta != null)
                return RedirectToAction("MiPanel", "Account");

            return View(consulta);
        }

        // POST: Ventas/PagarConfirm
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> PagarConfirm(int consultaId, string metodoPago)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            var dueno = await _context.Duenos
                .FirstOrDefaultAsync(d => d.Email != null && d.Email == user.Email && d.Activo);
            if (dueno == null) return Forbid();

            var consulta = await _context.Consultas
                .Include(c => c.Mascota)
                .Include(c => c.Medicamentos).ThenInclude(cm => cm.Producto)
                .Include(c => c.Venta)
                .FirstOrDefaultAsync(c => c.Id == consultaId && c.Activo);

            if (consulta == null) return NotFound();
            if (consulta.Mascota?.DuenoId != dueno.Id) return Forbid();
            if (consulta.Estado != "EnRevision" || consulta.Venta != null)
                return RedirectToAction("MiPanel", "Account");

            var totalMedicamentos = consulta.Medicamentos?.Sum(m => m.Subtotal) ?? 0;
            var numeroVenta = $"VTA-{DateTime.UtcNow:yyyyMMdd}-{consultaId}";

            var venta = new Venta
            {
                NumeroVenta = numeroVenta,
                ConsultaId = consultaId,
                DuenoId = dueno.Id,
                TotalConsulta = consulta.Costo,
                TotalMedicamentos = totalMedicamentos,
                Estado = "Pagada",
                MetodoPago = metodoPago,
                FechaVenta = DateTime.UtcNow,
                FechaPago = DateTime.UtcNow,
                Activo = true
            };

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            if (consulta.Medicamentos != null)
            {
                foreach (var med in consulta.Medicamentos)
                {
                    _context.DetallesVenta.Add(new DetalleVenta
                    {
                        VentaId = venta.Id,
                        ProductoId = med.ProductoId,
                        Cantidad = med.Cantidad,
                        PrecioUnitario = med.PrecioUnitario
                    });

                    var inventario = await _context.Inventarios
                        .FirstOrDefaultAsync(i => i.ProductoId == med.ProductoId);
                    if (inventario != null)
                    {
                        inventario.StockActual = Math.Max(0, inventario.StockActual - med.Cantidad);
                        inventario.FechaActualizacion = DateTime.UtcNow;
                    }
                }
            }

            consulta.Estado = "Completada";
            consulta.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction("Factura", new { ventaId = venta.Id });
        }

        // GET: Ventas/Factura?ventaId=5
        [HttpGet]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Factura(int? ventaId)
        {
            if (ventaId == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            var dueno = await _context.Duenos
                .FirstOrDefaultAsync(d => d.Email != null && d.Email == user.Email && d.Activo);
            if (dueno == null) return Forbid();

            var venta = await _context.Ventas
                .Include(v => v.Consulta).ThenInclude(c => c!.Mascota)
                .Include(v => v.Consulta).ThenInclude(c => c!.Veterinario)
                .Include(v => v.Dueno)
                .Include(v => v.Detalles).ThenInclude(dv => dv.Producto)
                .FirstOrDefaultAsync(v => v.Id == ventaId && v.Activo);

            if (venta == null) return NotFound();
            if (venta.DuenoId != dueno.Id) return Forbid();

            return View(venta);
        }
    }
}
