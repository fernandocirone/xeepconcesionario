using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using xeepconcesionario.Data;
using xeepconcesionario.Models;
using xeepconcesionario.Models.Dto;

namespace xeepconcesionario.Controllers
{
    public class ConfiguracionesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ConfiguracionesController(ApplicationDbContext context) => _context = context;

        // Vista principal
        public IActionResult Index() => View();

        // 🔄 Listado dinámico por tipo
        [HttpGet]
        public async Task<IActionResult> List(string tipo)
        {
            var items = await GetListAsync(tipo);
            return PartialView("_TablaLookup", items);
        }

        // 🧾 Modal de creación/edición (GET)
        [HttpGet]
        public async Task<IActionResult> UpsertForm(string tipo, int? id)
        {
            LookupDto dto = id is null
                ? new LookupDto { Id = 0, Tipo = tipo }
                : await GetByIdAsync(tipo, id.Value) ?? throw new ArgumentException("Elemento no encontrado");

            return PartialView("_UpsertLookup", dto);
        }

        // 💾 Guardar cambios (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(LookupDto dto)
        {
            if (!ModelState.IsValid)
                return PartialView("_UpsertLookup", dto);

            switch (dto.Tipo?.ToLowerInvariant())
            {
                case "estado":
                    await UpsertEstadoAsync(dto);
                    break;

                case "condicion":
                case "condicionventa":
                    await UpsertCondicionVentaAsync(dto);
                    break;

                case "tipobaja":
                    await UpsertTipoBajaAsync(dto);
                    break;

                case "estadoactividad":
                    await UpsertEstadoActividadAsync(dto);
                    break;

                default:
                    return BadRequest("Tipo no válido");
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // 🗑️ Eliminar registro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string tipo, int id)
        {
            switch (tipo?.ToLowerInvariant())
            {
                case "estado":
                    var e = await _context.Estados.FindAsync(id);
                    if (e == null) return NotFound();
                    _context.Estados.Remove(e);
                    break;

                case "condicion":
                case "condicionventa":
                    var c = await _context.CondicionesVenta.FindAsync(id);
                    if (c == null) return NotFound();
                    _context.CondicionesVenta.Remove(c);
                    break;

                case "tipobaja":
                    var t = await _context.TiposBaja.FindAsync(id);
                    if (t == null) return NotFound();
                    _context.TiposBaja.Remove(t);
                    break;

                case "estadoactividad":
                    var ea = await _context.EstadosActividad.FindAsync(id);
                    if (ea == null) return NotFound();
                    _context.EstadosActividad.Remove(ea);
                    break;

                default:
                    return BadRequest("Tipo no válido");
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // 🔧 Helpers

        private async Task<List<LookupDto>> GetListAsync(string tipo)
        {
            return tipo?.ToLowerInvariant() switch
            {
                "estado" => await _context.Estados
                    .Select(x => new LookupDto { Id = x.EstadoId, Nombre = x.NombreEstado, Tipo = "estado" })
                    .OrderBy(x => x.Nombre).ToListAsync(),

                "condicion" or "condicionventa" => await _context.CondicionesVenta
                    .Select(x => new LookupDto { Id = x.CondicionVentaId, Nombre = x.NombreCondicionVenta, Tipo = "condicion" })
                    .OrderBy(x => x.Nombre).ToListAsync(),

                "tipobaja" => await _context.TiposBaja
                    .Select(x => new LookupDto { Id = x.TipoBajaId, Nombre = x.NombreTipoBaja, Tipo = "tipobaja" })
                    .OrderBy(x => x.Nombre).ToListAsync(),

                "estadoactividad" => await _context.EstadosActividad
                    .Select(x => new LookupDto { Id = x.EstadoActividadId, Nombre = x.NombreEstadoActividad, Tipo = "estadoactividad" })
                    .OrderBy(x => x.Nombre).ToListAsync(),

                _ => new List<LookupDto>()
            };
        }

        private async Task<LookupDto?> GetByIdAsync(string tipo, int id)
        {
            return tipo?.ToLowerInvariant() switch
            {
                "estado" => await _context.Estados
                    .Where(x => x.EstadoId == id)
                    .Select(x => new LookupDto { Id = x.EstadoId, Nombre = x.NombreEstado, Tipo = "estado" })
                    .FirstOrDefaultAsync(),

                "condicion" or "condicionventa" => await _context.CondicionesVenta
                    .Where(x => x.CondicionVentaId == id)
                    .Select(x => new LookupDto { Id = x.CondicionVentaId, Nombre = x.NombreCondicionVenta, Tipo = "condicion" })
                    .FirstOrDefaultAsync(),

                "tipobaja" => await _context.TiposBaja
                    .Where(x => x.TipoBajaId == id)
                    .Select(x => new LookupDto { Id = x.TipoBajaId, Nombre = x.NombreTipoBaja, Tipo = "tipobaja" })
                    .FirstOrDefaultAsync(),

                "estadoactividad" => await _context.EstadosActividad
                    .Where(x => x.EstadoActividadId == id)
                    .Select(x => new LookupDto { Id = x.EstadoActividadId, Nombre = x.NombreEstadoActividad, Tipo = "estadoactividad" })
                    .FirstOrDefaultAsync(),

                _ => null
            };
        }

        private async Task UpsertEstadoAsync(LookupDto dto)
        {
            if (dto.Id == 0)
                _context.Estados.Add(new Estado { NombreEstado = dto.Nombre });
            else
            {
                var e = await _context.Estados.FindAsync(dto.Id);
                if (e == null) throw new ArgumentException("Estado no encontrado");
                e.NombreEstado = dto.Nombre;
                _context.Estados.Update(e);
            }
        }

        private async Task UpsertCondicionVentaAsync(LookupDto dto)
        {
            if (dto.Id == 0)
                _context.CondicionesVenta.Add(new CondicionVenta { NombreCondicionVenta = dto.Nombre });
            else
            {
                var c = await _context.CondicionesVenta.FindAsync(dto.Id);
                if (c == null) throw new ArgumentException("Condición no encontrada");
                c.NombreCondicionVenta = dto.Nombre;
                _context.CondicionesVenta.Update(c);
            }
        }

        private async Task UpsertTipoBajaAsync(LookupDto dto)
        {
            if (dto.Id == 0)
                _context.TiposBaja.Add(new TipoBaja { NombreTipoBaja = dto.Nombre });
            else
            {
                var t = await _context.TiposBaja.FindAsync(dto.Id);
                if (t == null) throw new ArgumentException("Tipo de baja no encontrado");
                t.NombreTipoBaja = dto.Nombre;
                _context.TiposBaja.Update(t);
            }
        }

        private async Task UpsertEstadoActividadAsync(LookupDto dto)
        {
            if (dto.Id == 0)
                _context.EstadosActividad.Add(new EstadoActividad { NombreEstadoActividad = dto.Nombre });
            else
            {
                var ea = await _context.EstadosActividad.FindAsync(dto.Id);
                if (ea == null) throw new ArgumentException("EstadoActividad no encontrado");
                ea.NombreEstadoActividad = dto.Nombre;
                _context.EstadosActividad.Update(ea);
            }
        }
    }
}
