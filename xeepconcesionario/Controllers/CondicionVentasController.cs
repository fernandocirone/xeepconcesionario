using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using xeepconcesionario.Data;

namespace xeepconcesionario.Controllers
{
    public class CondicionVentasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CondicionVentasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CondicionVentas
        public async Task<IActionResult> Index()
        {
            return View(await _context.CondicionesVenta.ToListAsync());
        }

        // GET: CondicionVentas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var condicionVenta = await _context.CondicionesVenta
                .FirstOrDefaultAsync(m => m.CondicionVentaId == id);
            if (condicionVenta == null)
            {
                return NotFound();
            }

            return View(condicionVenta);
        }

        // GET: CondicionVentas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CondicionVentas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CondicionVentaId,NombreCondicionVenta")] CondicionVenta condicionVenta)
        {

                _context.Add(condicionVenta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

        }

        // GET: CondicionVentas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var condicionVenta = await _context.CondicionesVenta.FindAsync(id);
            if (condicionVenta == null)
            {
                return NotFound();
            }
            return View(condicionVenta);
        }

        // POST: CondicionVentas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CondicionVentaId,NombreCondicionVenta")] CondicionVenta condicionVenta)
        {
            if (id != condicionVenta.CondicionVentaId)
            {
                return NotFound();
            }


                try
                {
                    _context.Update(condicionVenta);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CondicionVentaExists(condicionVenta.CondicionVentaId))
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

        // GET: CondicionVentas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var condicionVenta = await _context.CondicionesVenta
                .FirstOrDefaultAsync(m => m.CondicionVentaId == id);
            if (condicionVenta == null)
            {
                return NotFound();
            }

            return View(condicionVenta);
        }

        // POST: CondicionVentas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var condicionVenta = await _context.CondicionesVenta.FindAsync(id);
            if (condicionVenta != null)
            {
                _context.CondicionesVenta.Remove(condicionVenta);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CondicionVentaExists(int id)
        {
            return _context.CondicionesVenta.Any(e => e.CondicionVentaId == id);
        }
    }
}
