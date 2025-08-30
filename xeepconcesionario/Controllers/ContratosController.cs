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
    public class ContratosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContratosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Contratos
        public async Task<IActionResult> Index()
        {
            return View(await _context.Contratos.ToListAsync());
        }

        [HttpPost]
        public IActionResult CrearDesdeSolicitud([FromForm] Contrato contrato)
        {

                _context.Contratos.Add(contrato);
                _context.SaveChanges();

                return Json(new { success = true, id = contrato.ContratoId, nombre = contrato.NombreContrato });

        }

        // GET: Contratos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contrato = await _context.Contratos
                .FirstOrDefaultAsync(m => m.ContratoId == id);
            if (contrato == null)
            {
                return NotFound();
            }

            return View(contrato);
        }

        // GET: Contratos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Contratos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ContratoId,NombreContrato,DescripcionContrato,PlazoMeses")] Contrato contrato)
        {

                _context.Add(contrato);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
        }

        // GET: Contratos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contrato = await _context.Contratos.FindAsync(id);
            if (contrato == null)
            {
                return NotFound();
            }
            return View(contrato);
        }

        // POST: Contratos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ContratoId,NombreContrato,DescripcionContrato,PlazoMeses")] Contrato contrato)
        {
            if (id != contrato.ContratoId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contrato);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContratoExists(contrato.ContratoId))
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
            return View(contrato);
        }

        // GET: Contratos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contrato = await _context.Contratos
                .FirstOrDefaultAsync(m => m.ContratoId == id);
            if (contrato == null)
            {
                return NotFound();
            }

            return View(contrato);
        }

        // POST: Contratos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contrato = await _context.Contratos.FindAsync(id);
            if (contrato != null)
            {
                _context.Contratos.Remove(contrato);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContratoExists(int id)
        {
            return _context.Contratos.Any(e => e.ContratoId == id);
        }
    }
}
