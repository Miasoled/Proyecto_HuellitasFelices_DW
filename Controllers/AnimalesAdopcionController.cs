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
    public class AnimalesAdopcionController : Controller
    {
        private readonly AppDbContext _context;

        public AnimalesAdopcionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: AnimalesAdopcion
        public async Task<IActionResult> Index()
        {
            return View(await _context.AnimalesAdopcion.ToListAsync());
        }

        // GET: AnimalesAdopcion/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animalAdopcion = await _context.AnimalesAdopcion
                .FirstOrDefaultAsync(m => m.Id == id);
            if (animalAdopcion == null)
            {
                return NotFound();
            }

            return View(animalAdopcion);
        }

        // GET: AnimalesAdopcion/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AnimalesAdopcion/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Especie,Raza,EdadAproximada,Descripcion,Disponible,Activo,FechaCreacion")] AnimalAdopcion animalAdopcion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(animalAdopcion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(animalAdopcion);
        }

        // GET: AnimalesAdopcion/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animalAdopcion = await _context.AnimalesAdopcion.FindAsync(id);
            if (animalAdopcion == null)
            {
                return NotFound();
            }
            return View(animalAdopcion);
        }

        // POST: AnimalesAdopcion/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Especie,Raza,EdadAproximada,Descripcion,Disponible,Activo,FechaCreacion")] AnimalAdopcion animalAdopcion)
        {
            if (id != animalAdopcion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(animalAdopcion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnimalAdopcionExists(animalAdopcion.Id))
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
            return View(animalAdopcion);
        }

        // GET: AnimalesAdopcion/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animalAdopcion = await _context.AnimalesAdopcion
                .FirstOrDefaultAsync(m => m.Id == id);
            if (animalAdopcion == null)
            {
                return NotFound();
            }

            return View(animalAdopcion);
        }

        // POST: AnimalesAdopcion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var animalAdopcion = await _context.AnimalesAdopcion.FindAsync(id);
            if (animalAdopcion != null)
            {
                _context.AnimalesAdopcion.Remove(animalAdopcion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AnimalAdopcionExists(int id)
        {
            return _context.AnimalesAdopcion.Any(e => e.Id == id);
        }
    }
}
