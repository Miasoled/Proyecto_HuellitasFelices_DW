using System.Diagnostics;
using HuellitasFelices.Data;
using Microsoft.AspNetCore.Mvc;
using HuellitasFelices.Models;
using Microsoft.EntityFrameworkCore;

namespace HuellitasFelices.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalMascotas = await _context.Mascotas.CountAsync(m => m.Activo);
            ViewBag.TotalDuenos = await _context.Duenos.CountAsync(d => d.Activo);
            ViewBag.TotalConsultas = await _context.Consultas.CountAsync(c => c.Activo);
            ViewBag.AnimalesAdopcion = await _context.AnimalesAdopcion.CountAsync(a => a.Activo && a.Disponible);
            return View();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
