using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using xeepconcesionario.Data;
using xeepconcesionario.Models;

namespace xeepconcesionario.Controllers
{
    public class ActividadesSolicitudController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActividadesSolicitudController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ActividadesSolicitud
        public async Task<IActionResult> Index(int? solicitudId)
        {
            var q = _context.ActividadesSolicitud
                .Include(a => a.Solicitud)
                .Include(a => a.EstadoActividad)
                .Include(a => a.Usuario)
                .AsNoTracking()
                .AsQueryable();

            if (solicitudId.HasValue)
                q = q.Where(a => a.SolicitudId == solicitudId.Value);

            return View(await q
                .OrderByDescending(a => a.Fecha)
                .ToListAsync());
        }

        // GET: ActividadesSolicitud/Create
        public async Task<IActionResult> Create(int solicitudId)
        {
            ViewBag.SolicitudId = solicitudId;
            ViewData["EstadoActividadId"] = new SelectList(
                await _context.EstadosActividad.AsNoTracking().ToListAsync(),
                "EstadoActividadId", "NombreEstadoActividad"
            );

            return View(new ActividadSolicitud { SolicitudId = solicitudId });
        }

        // POST: ActividadesSolicitud/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ActividadSolicitud actividad)
        {
            if (ModelState.IsValid)
            {
                actividad.Fecha = DateTime.Now;
                actividad.UsuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                _context.Add(actividad);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Solicitudes", new { id = actividad.SolicitudId });
            }

            ViewData["EstadoActividadId"] = new SelectList(
                _context.EstadosActividad, "EstadoActividadId", "NombreEstadoActividad", actividad.EstadoActividadId
            );
            return View(actividad);
        }

        // GET: ActividadesSolicitud/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var actividad = await _context.ActividadesSolicitud
                .Include(a => a.EstadoActividad)
                .Include(a => a.Solicitud)
                .Include(a => a.Usuario)
                .FirstOrDefaultAsync(m => m.ActividadSolicitudId == id);

            if (actividad == null) return NotFound();

            return View(actividad);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFromDetails(int SolicitudId, int EstadoActividadId, string? Observacion)
        {
            var actividad = new ActividadSolicitud
            {
                SolicitudId = SolicitudId,
                EstadoActividadId = EstadoActividadId,
                Observacion = Observacion,
                Fecha = DateTime.Now,
                UsuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier)!
            };

            _context.ActividadesSolicitud.Add(actividad);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Solicitudes", new { id = SolicitudId });
        }


        // POST: ActividadesSolicitud/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actividad = await _context.ActividadesSolicitud.FindAsync(id);
            if (actividad != null)
            {
                _context.ActividadesSolicitud.Remove(actividad);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Details", "Solicitudes", new { id = actividad?.SolicitudId });
        }
    }
}
