using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using xeepconcesionario.Data;
using xeepconcesionario.Models;

public class ActividadesVehiculoController : Controller
{
    private readonly ApplicationDbContext _context;

    public ActividadesVehiculoController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(
        int vehiculoId,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        int? tipoActividadId,
        string? sucursal)
    {
        var query = _context.ActividadesVehiculo
            .Include(a => a.TipoActividadVehiculo)
            .Include(a => a.Sucursal)
            .Include(a => a.Usuario)
            .Where(a => a.VehiculoId == vehiculoId);

        if (fechaDesde.HasValue)
            query = query.Where(a => a.Fecha >= fechaDesde.Value);
        if (fechaHasta.HasValue)
            query = query.Where(a => a.Fecha <= fechaHasta.Value);
        if (tipoActividadId.HasValue)
            query = query.Where(a => a.TipoActividadVehiculoId == tipoActividadId);
        if (!string.IsNullOrWhiteSpace(sucursal))
            query = query.Where(a => a.Sucursal!.NombreSucursal.Contains(sucursal));

        ViewBag.Vehiculo = await _context.Vehiculos.FindAsync(vehiculoId);
        ViewBag.FechaDesde = fechaDesde?.ToString("yyyy-MM-dd");
        ViewBag.FechaHasta = fechaHasta?.ToString("yyyy-MM-dd");
        ViewBag.TipoActividadId = tipoActividadId;
        ViewBag.Sucursal = sucursal;

        // Combos para el modal
        ViewBag.TipoActividadVehiculoId = new SelectList(_context.TiposActividadVehiculo, "Id", "NombreTipoActividadVehiculo");
        ViewBag.SucursalId = new SelectList(_context.Sucursales, "Id", "NombreSucursal");

        return View(await query.OrderByDescending(a => a.Fecha).ToListAsync());
    }

    public async Task<IActionResult> IndexTodos(
    DateTime? fechaDesde,
    DateTime? fechaHasta,
    int? tipoActividadId,
    string? sucursal)
    {
        var query = _context.ActividadesVehiculo
            .Include(a => a.TipoActividadVehiculo)
            .Include(a => a.Sucursal)
            .Include(a => a.Usuario)
            .Include(a => a.Vehiculo)  // importante para mostrar modelo/patente
            .AsQueryable();

        if (fechaDesde.HasValue)
            query = query.Where(a => a.Fecha >= fechaDesde.Value);
        if (fechaHasta.HasValue)
            query = query.Where(a => a.Fecha <= fechaHasta.Value);
        if (tipoActividadId.HasValue)
            query = query.Where(a => a.TipoActividadVehiculoId == tipoActividadId);
        if (!string.IsNullOrWhiteSpace(sucursal))
            query = query.Where(a => a.Sucursal!.NombreSucursal.Contains(sucursal));

        ViewBag.FechaDesde = fechaDesde?.ToString("yyyy-MM-dd");
        ViewBag.FechaHasta = fechaHasta?.ToString("yyyy-MM-dd");
        ViewBag.TipoActividadId = tipoActividadId;
        ViewBag.Sucursal = sucursal;

        return View(await query.OrderByDescending(a => a.Fecha).ToListAsync());
    }



    // POST: guarda la nueva actividad
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ActividadVehiculo actividad)
    {
        // Asignar usuario logueado
        actividad.UsuarioId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        actividad.Fecha = actividad.Fecha == default ? DateTime.Today : actividad.Fecha;

        // Quitamos validación para UsuarioId, ya que lo seteo manual
        ModelState.Remove(nameof(ActividadVehiculo.UsuarioId));

        if (ModelState.IsValid)
        {
            _context.Add(actividad);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { vehiculoId = actividad.VehiculoId });
        }

        // Si falla, volver al index
        return RedirectToAction(nameof(Index), new { vehiculoId = actividad.VehiculoId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ActividadVehiculo actividad)
    {
        if (id != actividad.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                actividad.UsuarioId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
                _context.Update(actividad);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ActividadesVehiculo.Any(a => a.Id == actividad.Id))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index), new { vehiculoId = actividad.VehiculoId });
        }

        // Si hay error, recargar combos
        ViewBag.TipoActividadVehiculoId = new SelectList(_context.TiposActividadVehiculo, "Id", "NombreTipoActividadVehiculo", actividad.TipoActividadVehiculoId);
        ViewBag.SucursalId = new SelectList(_context.Sucursales, "Id", "NombreSucursal", actividad.SucursalId);
        return View(actividad);
    }


}
