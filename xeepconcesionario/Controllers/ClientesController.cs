using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using xeepconcesionario.Data;

namespace xeepconcesionario.Controllers
{
    [Authorize]
    public class ClientesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Clientes
        public async Task<IActionResult> Index(string ApellidoYNombre, string Dni, string Mail, string Region)
        {
            var query = _context.Clientes
                .Include(c => c.Localidad)
                    .ThenInclude(l => l.Region)
                .Include(c => c.Localidad)
                    .ThenInclude(l => l.Provincia)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(ApellidoYNombre))
                query = query.Where(c => c.ApellidoYNombre.ToLower().Contains(ApellidoYNombre.ToLower()));

            if (!string.IsNullOrWhiteSpace(Dni))
                query = query.Where(c => c.Dni.ToLower().Contains(Dni.ToLower()));

            if (!string.IsNullOrWhiteSpace(Mail))
                query = query.Where(c => c.Mail.ToLower().Contains(Mail.ToLower()));

            if (!string.IsNullOrWhiteSpace(Region))
                query = query.Where(c => c.Localidad.Region.NombreRegion.ToLower().Contains(Region.ToLower()));

            var clientes = await query.ToListAsync();

            ViewBag.LocalidadId = new SelectList(_context.Localidades, "LocalidadId", "NombreLocalidad");

            return View(clientes);
        }


        public async Task<IActionResult> FiltrarClientes(string nombre, string dni, string region)
        {
            var query = _context.Clientes
                .Include(c => c.Localidad)
                    .ThenInclude(l => l.Region)
                .AsQueryable();

            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(c => c.ApellidoYNombre.Contains(nombre));

            if (!string.IsNullOrEmpty(dni))
                query = query.Where(c => c.Dni.Contains(dni));

            if (!string.IsNullOrEmpty(region))
                query = query.Where(c => c.Localidad.Region.NombreRegion.Contains(region));

            var clientesFiltrados = await query.ToListAsync();
            return PartialView("_TablaClientes", clientesFiltrados);
        }




        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clientes
                .Include(c => c.Localidad)
                .Include(c => c.Solicitudes)
                    .ThenInclude(s => s.Plan)
                .Include(c => c.Solicitudes)
                    .ThenInclude(s => s.Estado)
                .FirstOrDefaultAsync(c => c.ClienteId == id);

            if (cliente == null) return NotFound();

            return View(cliente);
        }


        // GET: Clientes/Create
        public IActionResult Create()
        {
            ViewData["LocalidadId"] = new SelectList(_context.Localidades, "LocalidadId", "NombreLocalidad");
            return View();
        }

        // POST: Clientes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClienteId,ApellidoYNombre,Dni,TelefonoFijo,TelefonoCelular,Mail,FechaNacimiento,Direccion,Nacionalidad,LocalidadId,Barrio,TipoVivienda,TieneTarjetaCredito,Sexo,EstadoCivil,Ocupacion,Empresa,DomicilioLaboral,Cargo,IngresosMensuales,TipoOcupacion,RazonSocial")] Cliente cliente)
        {

                _context.Add(cliente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

        }

        // GET: Clientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }
            ViewData["LocalidadId"] = new SelectList(_context.Localidades, "LocalidadId", "NombreLocalidad", cliente.LocalidadId);
            return View(cliente);
        }

        // POST: Clientes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ClienteId,ApellidoYNombre,Dni,TelefonoFijo,TelefonoCelular,Mail,FechaNacimiento,Direccion,Nacionalidad,LocalidadId,Barrio,TipoVivienda,TieneTarjetaCredito,Sexo,EstadoCivil,Ocupacion,Empresa,DomicilioLaboral,Cargo,IngresosMensuales,TipoOcupacion,RazonSocial")] Cliente cliente)
        {
            if (id != cliente.ClienteId)
            {
                return NotFound();
            }


                try
                {
                    _context.Update(cliente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClienteExists(cliente.ClienteId))
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

        // GET: Clientes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .Include(c => c.Localidad)
                .FirstOrDefaultAsync(m => m.ClienteId == id);
            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // POST: Clientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.ClienteId == id);
        }
    }
}
