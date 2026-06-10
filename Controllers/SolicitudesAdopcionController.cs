using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HuellitasFelices.Data;
using HuellitasFelices.Models;

namespace HuellitasFelices.Controllers
{
    public class SolicitudesAdopcionController : Controller
    {
        private readonly AppDbContext _context;

        public SolicitudesAdopcionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: SolicitudesAdopcion
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.SolicitudesAdopcion.Include(s => s.AnimalAdopcion);
            return View(await appDbContext.ToListAsync());
        }

        // GET: SolicitudesAdopcion/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var solicitudAdopcion = await _context.SolicitudesAdopcion
                .Include(s => s.AnimalAdopcion)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (solicitudAdopcion == null)
            {
                return NotFound();
            }

            return View(solicitudAdopcion);
        }

        // GET: SolicitudesAdopcion/Create
        public IActionResult Create()
        {
            ViewData["AnimalAdopcionId"] = new SelectList(_context.AnimalesAdopcion, "Id", "Especie");
            return View();
        }

        // POST: SolicitudesAdopcion/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NombreSolicitante,Telefono,Email,Estado,FechaSolicitud,Activo,FechaCreacion,AnimalAdopcionId")] SolicitudAdopcion solicitudAdopcion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(solicitudAdopcion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AnimalAdopcionId"] = new SelectList(_context.AnimalesAdopcion, "Id", "Especie", solicitudAdopcion.AnimalAdopcionId);
            return View(solicitudAdopcion);
        }

        // GET: SolicitudesAdopcion/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var solicitudAdopcion = await _context.SolicitudesAdopcion.FindAsync(id);
            if (solicitudAdopcion == null)
            {
                return NotFound();
            }
            ViewData["AnimalAdopcionId"] = new SelectList(_context.AnimalesAdopcion, "Id", "Especie", solicitudAdopcion.AnimalAdopcionId);
            return View(solicitudAdopcion);
        }

        // POST: SolicitudesAdopcion/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombreSolicitante,Telefono,Email,Estado,FechaSolicitud,Activo,FechaCreacion,AnimalAdopcionId")] SolicitudAdopcion solicitudAdopcion)
        {
            if (id != solicitudAdopcion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(solicitudAdopcion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SolicitudAdopcionExists(solicitudAdopcion.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AnimalAdopcionId"] = new SelectList(_context.AnimalesAdopcion, "Id", "Especie", solicitudAdopcion.AnimalAdopcionId);
            return View(solicitudAdopcion);
        }

        // GET: SolicitudesAdopcion/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var solicitudAdopcion = await _context.SolicitudesAdopcion
                .Include(s => s.AnimalAdopcion)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (solicitudAdopcion == null)
            {
                return NotFound();
            }

            return View(solicitudAdopcion);
        }

        // POST: SolicitudesAdopcion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var solicitudAdopcion = await _context.SolicitudesAdopcion.FindAsync(id);
            if (solicitudAdopcion != null)
            {
                _context.SolicitudesAdopcion.Remove(solicitudAdopcion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SolicitudAdopcionExists(int id)
        {
            return _context.SolicitudesAdopcion.Any(e => e.Id == id);
        }
    }
}
