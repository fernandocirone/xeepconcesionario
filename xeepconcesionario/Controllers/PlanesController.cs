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
    public class PlanesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlanesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Planes
        public async Task<IActionResult> Index()
        {
            return View(await _context.Planes.ToListAsync());
        }

        public async Task<IActionResult> FiltrarPlanes(string modelo, string codigo)
        {
            var query = _context.Planes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(modelo))
                query = query.Where(a => a.Modelo.ToLower().Contains(modelo.ToLower()));

            if (!string.IsNullOrWhiteSpace(codigo))
                query = query.Where(a => a.Codigo.ToLower().Contains(codigo.ToLower()));

            var lista = await query.ToListAsync();
            return PartialView("_TablaPlanes", lista);
        }

        // PlanesController.cs
        [HttpGet("/Planes/sellados/{id:int}")]
        public async Task<IActionResult> GetSellados(int id)
        {
            var datos = await _context.Planes
                .Where(a => a.PlanId == id)
                .Select(a => new
                {
                    sellado1 = a.Sellado,
                    sellado2 = a.Sellado,
                    importeCuota = a.CuotaIngreso   // 👈 agregado acá
                })
                .FirstOrDefaultAsync();

            if (datos is null)
                return NotFound();

            return Json(datos);
        }




        // GET: Planes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Plan = await _context.Planes
                .FirstOrDefaultAsync(m => m.PlanId == id);
            if (Plan == null)
            {
                return NotFound();
            }

            return View(Plan);
        }

        // GET: Planes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Planes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PlanId,Codigo,Modelo,CuotaApertura,AdelantoMensual,Sellado,CuotaIngreso")] Plan Plan)
        {

                _context.Add(Plan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
        }

        // GET: Planes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Plan = await _context.Planes.FindAsync(id);
            if (Plan == null)
            {
                return NotFound();
            }
            return View(Plan);
        }

        // POST: Planes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PlanId,Codigo,Modelo,CuotaApertura,AdelantoMensual,Sellado,CuotaIngreso")] Plan Plan)
        {
            if (id != Plan.PlanId)
            {
                return NotFound();
            }


                try
                {
                    _context.Update(Plan);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlanExists(Plan.PlanId))
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

        // GET: Planes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Plan = await _context.Planes
                .FirstOrDefaultAsync(m => m.PlanId == id);
            if (Plan == null)
            {
                return NotFound();
            }

            return View(Plan);
        }

        // POST: Planes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var Plan = await _context.Planes.FindAsync(id);
            if (Plan != null)
            {
                _context.Planes.Remove(Plan);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PlanExists(int id)
        {
            return _context.Planes.Any(e => e.PlanId == id);
        }
    }
}
