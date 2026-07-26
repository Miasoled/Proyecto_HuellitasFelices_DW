using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HuellitasFelices.Data;
using HuellitasFelices.Models;

namespace HuellitasFelices.Controllers
{
    public class TiendaController : Controller
    {
        private readonly AppDbContext _context;
        private const int TamanioPagina = 12;

        public TiendaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Tienda — público
        [AllowAnonymous]
        public async Task<IActionResult> Index(int pagina = 1, string? busqueda = null, int? categoriaId = null)
        {
            var consulta = _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Include(p => p.Inventarios)
                .Where(p => p.Activo)
                .AsQueryable();

            if (categoriaId.HasValue)
                consulta = consulta.Where(p => p.CategoriaId == categoriaId.Value);

            if (!string.IsNullOrEmpty(busqueda))
                consulta = consulta.Where(p =>
                    EF.Functions.ILike(p.Nombre, $"%{busqueda}%") ||
                    (p.Descripcion != null && EF.Functions.ILike(p.Descripcion, $"%{busqueda}%")));

            var totalRegistros = await consulta.CountAsync();
            var productos = await consulta
                .OrderByDescending(p => p.FechaCreacion)
                .Skip((pagina - 1) * TamanioPagina)
                .Take(TamanioPagina)
                .ToListAsync();

            ViewBag.Paginacion = new PaginacionViewModel
            {
                PaginaActual   = pagina,
                TotalPaginas   = (int)Math.Ceiling(totalRegistros / (double)TamanioPagina),
                TotalRegistros = totalRegistros,
                TamanioPagina  = TamanioPagina,
                Busqueda       = busqueda
            };

            ViewBag.Categorias = await _context.Categorias
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            ViewBag.CategoriaSeleccionada = categoriaId;

            return View(productos);
        }

        // GET: Tienda/Detalle/5 — público
        [AllowAnonymous]
        public async Task<IActionResult> Detalle(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Include(p => p.Inventarios)
                .FirstOrDefaultAsync(p => p.Id == id && p.Activo);

            if (producto == null) return NotFound();

            var stockTotal = producto.Inventarios?.Sum(i => i.StockActual) ?? 0;

            ViewBag.StockTotal = stockTotal;

            var productosRelacionados = await _context.Productos
                .AsNoTracking()
                .Include(p => p.Inventarios)
                .Where(p => p.Activo && p.CategoriaId == producto.CategoriaId && p.Id != producto.Id)
                .Take(4)
                .ToListAsync();

            ViewBag.ProductosRelacionados = productosRelacionados;

            return View(producto);
        }

        // API: productos destacados para el panel del cliente
        [HttpGet]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Destacados()
        {
            var productos = await _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Include(p => p.Inventarios)
                .Where(p => p.Activo && p.Inventarios.Any(i => i.StockActual > 0))
                .OrderByDescending(p => p.FechaCreacion)
                .Take(6)
                .ToListAsync();

            return PartialView("_TiendaDestacados", productos);
        }
    }
}
