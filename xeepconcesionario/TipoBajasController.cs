using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using xeepconcesionario.Data;

namespace xeepconcesionario
{
    public class TipoBajasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TipoBajasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TipoBajas
        public async Task<IActionResult> Index()
        {
            return View(await _context.TiposBaja.ToListAsync());
        }

        // GET: TipoBajas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoBaja = await _context.TiposBaja
                .FirstOrDefaultAsync(m => m.TipoBajaId == id);
            if (tipoBaja == null)
            {
                return NotFound();
            }

            return View(tipoBaja);
        }

        // GET: TipoBajas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TipoBajas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TipoBajaId,NombreTipoBaja")] TipoBaja tipoBaja)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tipoBaja);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tipoBaja);
        }

        // GET: TipoBajas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoBaja = await _context.TiposBaja.FindAsync(id);
            if (tipoBaja == null)
            {
                return NotFound();
            }
            return View(tipoBaja);
        }

        // POST: TipoBajas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TipoBajaId,NombreTipoBaja")] TipoBaja tipoBaja)
        {
            if (id != tipoBaja.TipoBajaId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tipoBaja);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TipoBajaExists(tipoBaja.TipoBajaId))
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
            return View(tipoBaja);
        }

        // GET: TipoBajas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoBaja = await _context.TiposBaja
                .FirstOrDefaultAsync(m => m.TipoBajaId == id);
            if (tipoBaja == null)
            {
                return NotFound();
            }

            return View(tipoBaja);
        }

        // POST: TipoBajas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tipoBaja = await _context.TiposBaja.FindAsync(id);
            if (tipoBaja != null)
            {
                _context.TiposBaja.Remove(tipoBaja);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TipoBajaExists(int id)
        {
            return _context.TiposBaja.Any(e => e.TipoBajaId == id);
        }
    }
}
