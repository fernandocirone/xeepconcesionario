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
    public class CuotasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CuotasController(ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: Cuotas
        public async Task<IActionResult> Index(int? solicitudId)
        {
            var query = _context.Cuotas
                .Include(c => c.Solicitud)
                    .ThenInclude(s => s.Cliente)
                .AsNoTracking()
                .AsQueryable();

            if (solicitudId.HasValue)
            {
                query = query.Where(c => c.SolicitudId == solicitudId.Value);
                ViewBag.SolicitudId = solicitudId.Value;

                // Traer la Solicitud para el encabezado
                var solicitud = await _context.Solicitudes
                    .Include(s => s.Cliente)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.SolicitudId == solicitudId.Value);

                ViewBag.Solicitud = solicitud; // 👈 ahora existe en la vista
            }

            var cuotas = await query
                .OrderBy(c => c.Numerocuota)
                .ToListAsync();

            return View(cuotas);
        }




        // GET: Cuotas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuota = await _context.Cuotas
                .Include(c => c.Solicitud)
                .FirstOrDefaultAsync(m => m.CuotaId == id);
            if (cuota == null)
            {
                return NotFound();
            }

            return View(cuota);
        }

        // GET: Cuotas/Create
        public IActionResult Create()
        {
            ViewData["SolicitudId"] = new SelectList(_context.Solicitudes, "SolicitudId", "SolicitudId");
            return View();
        }

        // POST: Cuotas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CuotaId,SolicitudId,NumerocuotaId,PlazoMeses,MontoCuota,SaldoCuota,FechaVencimiento,FechaPago")] Cuota cuota)
        { 
                _context.Add(cuota);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));


        }

        // GET: Cuotas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuota = await _context.Cuotas.FindAsync(id);
            if (cuota == null)
            {
                return NotFound();
            }
            ViewData["SolicitudId"] = new SelectList(_context.Solicitudes, "SolicitudId", "SolicitudId", cuota.SolicitudId);
            return View(cuota);
        }

        // POST: Cuotas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CuotaId,SolicitudId,NumerocuotaId,PlazoMeses,MontoCuota,SaldoCuota,FechaVencimiento,FechaPago")] Cuota cuota)
        {
            if (id != cuota.CuotaId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cuota);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CuotaExists(cuota.CuotaId))
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
            ViewData["SolicitudId"] = new SelectList(_context.Solicitudes, "SolicitudId", "SolicitudId", cuota.SolicitudId);
            return View(cuota);
        }

        // GET: Cuotas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuota = await _context.Cuotas
                .Include(c => c.Solicitud)
                .FirstOrDefaultAsync(m => m.CuotaId == id);
            if (cuota == null)
            {
                return NotFound();
            }

            return View(cuota);
        }

        // POST: Cuotas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cuota = await _context.Cuotas.FindAsync(id);
            if (cuota != null)
            {
                _context.Cuotas.Remove(cuota);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CuotaExists(int id)
        {
            return _context.Cuotas.Any(e => e.CuotaId == id);
        }
    }
}
