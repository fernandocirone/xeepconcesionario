using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using xeepconcesionario.Data;
using xeepconcesionario.Models;
using xeepconcesionario.Models.Dto;

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
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CrearDesdeSolicitud([FromBody] ContratoDto dto)
        {
            var solicitud = await _context.Solicitudes
                .FirstOrDefaultAsync(s => s.SolicitudId == dto.SolicitudId);

            if (solicitud == null)
                return Json(new { success = false, message = "Solicitud inválida" });

            var vehiculo = await _context.Vehiculos
                .FirstOrDefaultAsync(v => v.Id == dto.VehiculoId);

            if (vehiculo == null)
                return Json(new { success = false, message = "Vehículo inválido" });

            // Buscar si ya existe contrato para esta solicitud
            var contrato = await _context.Contratos
                .FirstOrDefaultAsync(c => c.SolicitudId == solicitud.SolicitudId);

            if (contrato == null)
            {
                // Crear nuevo
                contrato = new Contrato
                {
                    SolicitudId = solicitud.SolicitudId,
                    VehiculoId = vehiculo.Id,
                    NombreContrato = dto.NombreContrato,
                    DescripcionContrato = dto.DescripcionContrato,
                    PlazoMeses = dto.PlazoMeses,
                    CantidadCuotas = dto.CantidadCuotas,
                    MontoCuota = dto.MontoCuota,
                    ValorTransferencia = dto.ValorTransferencia
                };

                _context.Contratos.Add(contrato);
            }
            else
            {
                // Editar existente
                contrato.VehiculoId = vehiculo.Id;
                contrato.NombreContrato = dto.NombreContrato;
                contrato.DescripcionContrato = dto.DescripcionContrato;
                contrato.PlazoMeses = dto.PlazoMeses;
                contrato.CantidadCuotas = dto.CantidadCuotas;
                contrato.MontoCuota = dto.MontoCuota;
                contrato.ValorTransferencia = dto.ValorTransferencia;

                _context.Contratos.Update(contrato);
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, contratoId = contrato.ContratoId });
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
