using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using xeepconcesionario.Data;
using xeepconcesionario.Models;

namespace xeepconcesionario.Controllers
{
    public class VehiculosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VehiculosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
           string? patente,
           string? modelo,
           int? año,
           string? color,
           DateTime? fechaDesde,
           DateTime? fechaHasta,
           int? tipo)
        {
            var query = _context.Vehiculos.AsQueryable();

            // Filtros
            if (!string.IsNullOrWhiteSpace(patente))
                query = query.Where(v => v.Patente.ToLower().Contains(patente.ToLower()));
            if (!string.IsNullOrWhiteSpace(modelo))
                query = query.Where(v => v.Modelo.ToLower().Contains(modelo.ToLower()));
            if (año.HasValue)
                query = query.Where(v => v.Año == año);
            if (!string.IsNullOrWhiteSpace(color))
                query = query.Where(v => v.Color.Contains(color));
            if (fechaDesde.HasValue)
                query = query.Where(v => v.FechaAlta >= fechaDesde);
            if (fechaHasta.HasValue)
                query = query.Where(v => v.FechaAlta <= fechaHasta);

            if (tipo.HasValue)
                query = query.Where(v => (int)v.Tipo == tipo.Value);

            // Traer con última actividad
            var vehiculos = await query
                .Select(v => new VehiculoIndexViewModel
                {
                    Id = v.Id,
                    Patente = v.Patente,
                    Tipo = v.Tipo,
                    Modelo = v.Modelo,
                    Año = v.Año,
                    Color = v.Color,
                    PrecioCompra = v.PrecioCompra,
                    Valor = v.Valor,
                    FechaAlta = v.FechaAlta,
                    Observacion = v.Observacion,

                    UltimaActividadNombre = v.Actividades
                        .OrderByDescending(a => a.Fecha)
                        .Select(a => a.TipoActividadVehiculo.NombreTipoActividadVehiculo)
                        .FirstOrDefault(),

                    UltimaActividadFecha = v.Actividades
                        .OrderByDescending(a => a.Fecha)
                        .Select(a => (DateTime?)a.Fecha)
                        .FirstOrDefault(),

                    Sucursal = v.Actividades
                        .OrderByDescending(a => a.Sucursal.NombreSucursal)
                        .Select(a => a.Sucursal.NombreSucursal)
                        .FirstOrDefault(),

                    // 👉 Nuevo campo
                    MontoActividad = v.Actividades.Sum(a => (decimal?)a.Monto) ?? 0

                })
                .ToListAsync();

            // Mantener filtros en ViewBag
            ViewBag.Patente = patente;
            ViewBag.Modelo = modelo;
            ViewBag.Año = año;
            ViewBag.Color = color;
            ViewBag.FechaDesde = fechaDesde;
            ViewBag.FechaHasta = fechaHasta;
            ViewBag.Tipo = tipo;


            return View(vehiculos);
        }



        // GET: Vehiculos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehiculo = await _context.Vehiculos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vehiculo == null)
            {
                return NotFound();
            }

            return View(vehiculo);
        }

        // GET: Vehiculos/Create
        public IActionResult Create()
        {
            ViewBag.SucursalId = new SelectList(_context.Sucursales, "Id", "NombreSucursal");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Patente,Tipo,Modelo,Año,Color,PrecioCompra,Valor,FechaAlta,Observacion")] Vehiculo vehiculo,
            int sucursalId
        )
        {
            if (ModelState.IsValid)
            {
                // Guardo el vehículo
                _context.Add(vehiculo);
                await _context.SaveChangesAsync();

                // Creo la actividad asociada
                var actividad = new ActividadVehiculo
                {
                    TipoActividadVehiculoId = 1, // Alta inicial
                    VehiculoId = vehiculo.Id,
                    SucursalId = sucursalId,     // 👈 viene del parámetro
                    Fecha = DateTime.Now,
                    UsuarioId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value
                };

                _context.Add(actividad);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Si falla, volvés a la vista
            ViewBag.SucursalId = new SelectList(_context.Sucursales, "Id", "NombreSucursal", sucursalId);
            return View(vehiculo);
        }



        // GET: Vehiculos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null)
            {
                return NotFound();
            }
            return View(vehiculo);
        }

        // POST: Vehiculos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Patente,Tipo,Modelo,Año,Color,PrecioCompra,Valor,FechaAlta,Observacion")] Vehiculo vehiculo)
        {
            if (id != vehiculo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vehiculo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VehiculoExists(vehiculo.Id))
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
            return View(vehiculo);
        }

        // GET: Vehiculos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehiculo = await _context.Vehiculos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vehiculo == null)
            {
                return NotFound();
            }

            return View(vehiculo);
        }

        // POST: Vehiculos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo != null)
            {
                _context.Vehiculos.Remove(vehiculo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VehiculoExists(int id)
        {
            return _context.Vehiculos.Any(e => e.Id == id);
        }
    }
}
