using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel; // <-- agregar
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using xeepconcesionario.Data;
using xeepconcesionario.Models;

namespace xeepconcesionario.Controllers
{
    public class CuotasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CuotasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // LISTADO GENERAL de CUOTAS (con filtros)
        // GET: /Cuotas/Index?fechaDesde=2025-09-01&fechaHasta=2026-09-01&estado=pendientes&...
        // =========================
        public async Task<IActionResult> Index(
            string? fechaDesde,
            string? fechaHasta,
            int? numeroCuota,
            string? estado,              // "todas" | "pendientes" | "pagadas"
            string? cliente,             // texto en ApellidoYNombre
            string? nombreSupervisor     // texto en Supervisor.NombreCompleto
        )
        {
            // Defaults de fechas (si no las pasan)
            var hoy = DateTime.Today;

            // Primer día del mes
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

            // Último día del mes
            var finMes = inicioMes.AddMonths(1).AddDays(-1);


            fechaDesde ??= inicioMes.ToString("yyyy-MM-dd");
            fechaHasta ??= finMes.ToString("yyyy-MM-dd");

            // Parseos seguros
            DateTime? dDesde = DateTime.TryParseExact(fechaDesde, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d1) ? d1 : (DateTime?)null;
            DateTime? dHasta = DateTime.TryParseExact(fechaHasta, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d2) ? d2 : (DateTime?)null;

            var query = _context.Cuotas
                .Include(c => c.Solicitud).ThenInclude(s => s.Cliente)
                .Include(c => c.Solicitud).ThenInclude(s => s.Supervisor)
                .Include(c => c.Solicitud).ThenInclude(s => s.Estado)
                .OrderBy(c => c.FechaVencimiento)
                .AsNoTracking()
                .AsQueryable();

            // Filtro fechas por Vencimiento
            if (dDesde.HasValue)
                query = query.Where(c => c.FechaVencimiento.Date >= dDesde.Value.Date);
            if (dHasta.HasValue)
                query = query.Where(c => c.FechaVencimiento.Date <= dHasta.Value.Date);

            // N° de cuota
            if (numeroCuota.HasValue && numeroCuota.Value > 0)
                query = query.Where(c => c.Numerocuota == numeroCuota.Value);

            // Estado
            estado = string.IsNullOrWhiteSpace(estado) ? "todas" : estado.ToLower();
            if (estado == "pendientes")
                query = query.Where(c => c.SaldoCuota > 0);
            else if (estado == "pagadas")
                query = query.Where(c => c.SaldoCuota == 0);

            // Cliente
            if (!string.IsNullOrWhiteSpace(cliente))
                query = query.Where(c => (c.Solicitud!.Cliente!.ApellidoYNombre ?? "")
                    .ToLower().Contains(cliente.ToLower()));

            // Supervisor
            if (!string.IsNullOrWhiteSpace(nombreSupervisor))
                query = query.Where(c => (c.Solicitud!.Supervisor!.NombreCompleto ?? "")
                    .ToLower().Contains(nombreSupervisor.ToLower()));

            var cuotas = await query
                .OrderBy(c => c.FechaVencimiento)
                .ThenBy(c => c.Numerocuota)
                .ToListAsync();

            // Persisto filtros para la vista
            ViewBag.FechaDesde = fechaDesde;
            ViewBag.FechaHasta = fechaHasta;
            ViewBag.NumeroCuota = numeroCuota?.ToString() ?? "";
            ViewBag.Estado = estado;
            ViewBag.Cliente = cliente ?? "";
            ViewBag.NombreSupervisor = nombreSupervisor ?? "";

            return View(cuotas); // -> Views/Cuotas/Index.cshtml (IEnumerable<Cuota>)
        }


        // =========================
        // LISTADO por SolicitudId (con filtros) EN DETAILS
        // GET: /Cuotas/Details?solicitudId=123&fechaDesde=2025-09-01&fechaHasta=2025-09-30&...
        // =========================
        public async Task<IActionResult> Details(
            int solicitudId,
            string? fechaDesde,
            string? fechaHasta,
            int? numeroCuota,
            string? estado,              // "todas" | "pendientes" | "pagadas"
            string? cliente,             // texto en ApellidoYNombre
            string? nombreSupervisor     // texto en Supervisor.NombreCompleto (si tenés esa relación)
        )
        {
            var hoy = DateTime.Today;

            // Primer día del mes
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

            // Último día del mes
            var finMes = inicioMes.AddYears(1).AddDays(-1);


            fechaDesde ??= inicioMes.ToString("yyyy-MM-dd");
            fechaHasta ??= finMes.ToString("yyyy-MM-dd");



            // Parseos seguros
            DateTime? dDesde = DateTime.TryParseExact(fechaDesde, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d1) ? d1 : (DateTime?)null;
            DateTime? dHasta = DateTime.TryParseExact(fechaHasta, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d2) ? d2 : (DateTime?)null;

            var query = _context.Cuotas
                .Include(c => c.Solicitud)
                    .ThenInclude(s => s.Cliente)
                .Include(c => c.Solicitud)
                    .ThenInclude(s => s.Supervisor)   // si existe la navegación
                .Include(c => c.Solicitud).ThenInclude(s => s.Estado)
                .AsNoTracking()
                .Where(c => c.SolicitudId == solicitudId)
                .AsQueryable();

            // Filtro fechas por Vencimiento
            if (dDesde.HasValue)
                query = query.Where(c => c.FechaVencimiento.Date >= dDesde.Value.Date);
            if (dHasta.HasValue)
                query = query.Where(c => c.FechaVencimiento.Date <= dHasta.Value.Date);

            // N° de cuota
            if (numeroCuota.HasValue && numeroCuota.Value > 0)
                query = query.Where(c => c.Numerocuota == numeroCuota.Value);

            // Estado
            estado = string.IsNullOrWhiteSpace(estado) ? "todas" : estado.ToLower();
            if (estado == "pendientes")
                query = query.Where(c => c.SaldoCuota > 0);
            else if (estado == "pagadas")
                query = query.Where(c => c.SaldoCuota == 0);

            // Cliente
            if (!string.IsNullOrWhiteSpace(cliente))
                query = query.Where(c => (c.Solicitud!.Cliente!.ApellidoYNombre ?? "")
                    .ToLower().Contains(cliente.ToLower()));

            // Supervisor
            if (!string.IsNullOrWhiteSpace(nombreSupervisor))
                query = query.Where(c => (c.Solicitud!.Supervisor!.NombreCompleto ?? "")
                    .ToLower().Contains(nombreSupervisor.ToLower()));

            var cuotas = await query
                .OrderBy(c => c.Numerocuota)
                .ToListAsync();

            // Para encabezado
            var solicitud = await _context.Solicitudes
                .Include(s => s.Cliente)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SolicitudId == solicitudId);

            ViewBag.SolicitudId = solicitudId;
            ViewBag.Solicitud = solicitud;

            // Persisto filtros para la vista
            ViewBag.FechaDesde = fechaDesde;
            ViewBag.FechaHasta = fechaHasta;
            ViewBag.NumeroCuota = numeroCuota?.ToString() ?? "";
            ViewBag.Estado = estado;
            ViewBag.Cliente = cliente ?? "";
            ViewBag.NombreSupervisor = nombreSupervisor ?? "";

            return View(cuotas); // -> Views/Cuotas/Details.cshtml (IEnumerable<Cuota>)
        }

        public async Task<IActionResult> ExportarExcel(
           string? fechaDesde,
           string? fechaHasta,
           int? numeroCuota,
           string? estado,
           string? cliente,
           string? nombreSupervisor
       )
        {
            // Defaults de fechas
            var hoy = DateTime.Today;

            // Primer día del mes
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

            // Último día del mes
            var finMes = inicioMes.AddMonths(1).AddDays(-1);


            fechaDesde ??= inicioMes.ToString("yyyy-MM-dd");
            fechaHasta ??= finMes.ToString("yyyy-MM-dd");

            DateTime? dDesde = DateTime.TryParseExact(fechaDesde, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d1) ? d1 : (DateTime?)null;
            DateTime? dHasta = DateTime.TryParseExact(fechaHasta, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d2) ? d2 : (DateTime?)null;

            var query = _context.Cuotas
                .Include(c => c.Solicitud).ThenInclude(s => s.Cliente)
                .Include(c => c.Solicitud).ThenInclude(s => s.Supervisor)
                .Include(c => c.Solicitud).ThenInclude(s => s.Estado)
                .AsNoTracking()
                .AsQueryable(); // 🔹 SIN filtro por SolicitudId

            if (dDesde.HasValue)
                query = query.Where(c => c.FechaVencimiento.Date >= dDesde.Value.Date);
            if (dHasta.HasValue)
                query = query.Where(c => c.FechaVencimiento.Date <= dHasta.Value.Date);

            if (numeroCuota.HasValue && numeroCuota.Value > 0)
                query = query.Where(c => c.Numerocuota == numeroCuota.Value);

            estado = string.IsNullOrWhiteSpace(estado) ? "todas" : estado.ToLower();
            if (estado == "pendientes")
                query = query.Where(c => c.SaldoCuota > 0);
            else if (estado == "pagadas")
                query = query.Where(c => c.SaldoCuota == 0);

            if (!string.IsNullOrWhiteSpace(cliente))
                query = query.Where(c => (c.Solicitud!.Cliente!.ApellidoYNombre ?? "")
                    .ToLower().Contains(cliente.ToLower()));

            if (!string.IsNullOrWhiteSpace(nombreSupervisor))
                query = query.Where(c => (c.Solicitud!.Supervisor!.NombreCompleto ?? "")
                    .ToLower().Contains(nombreSupervisor.ToLower()));

            var data = await query
                .OrderBy(c => c.SolicitudId)
                .ThenBy(c => c.Numerocuota)
                .ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Cuotas");

            // Encabezados
            ws.Cell(1, 1).Value = "Numero Solicitud";
            ws.Cell(1, 2).Value = "Estado";
            ws.Cell(1, 3).Value = "Cliente";
            ws.Cell(1, 4).Value = "Supervisor";
            ws.Cell(1, 5).Value = "N° Cuota";
            ws.Cell(1, 6).Value = "Monto";
            ws.Cell(1, 7).Value = "Saldo";
            ws.Cell(1, 8).Value = "Vencimiento";
            ws.Cell(1, 9).Value = "Fecha Pago";

            int row = 2;
            foreach (var c in data)
            {
                ws.Cell(row, 1).Value = c.Solicitud?.NumeroSolicitud;
                ws.Cell(row, 2).Value = c.Solicitud?.Estado.NombreEstado;
                ws.Cell(row, 3).Value = c.Solicitud?.Cliente?.ApellidoYNombre ?? "-";
                ws.Cell(row, 4).Value = c.Solicitud?.Supervisor?.NombreCompleto ?? "-";
                ws.Cell(row, 5).Value = c.Numerocuota;
                ws.Cell(row, 6).Value = c.MontoCuota;
                ws.Cell(row, 7).Value = c.SaldoCuota;
                ws.Cell(row, 8).Value = c.FechaVencimiento.ToString("dd/MM/yyyy");
                ws.Cell(row, 9).Value = c.FechaPago?.ToString("dd/MM/yyyy") ?? "-";
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new System.IO.MemoryStream();
            wb.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"Cuotas_General_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarMasivo(int solicitudId, decimal MontoNuevo, decimal SaldoNuevo)
        {
            var solicitud = await _context.Solicitudes
                .Include(s => s.Plan) // por si querés traer valores del auto
                .FirstOrDefaultAsync(s => s.SolicitudId == solicitudId);

            if (solicitud == null)
                return NotFound();

            var cuotas = await _context.Cuotas
                .Where(c => c.SolicitudId == solicitudId)
                .OrderBy(c => c.Numerocuota)
                .ToListAsync();

            if (cuotas.Any())
            {
                foreach (var cuota in cuotas)
                {
                    decimal monto = MontoNuevo;
                    decimal saldo = SaldoNuevo;

                    if (cuota.Numerocuota == 1)
                    {
                        monto += solicitud.ValorSellado1;
                        saldo += solicitud.ValorSellado1;
                    }
                    if (cuota.Numerocuota == 2)
                    {
                        monto += solicitud.ValorSellado2;
                        saldo += solicitud.ValorSellado2;
                    }

                    cuota.MontoCuota = monto;
                    cuota.SaldoCuota = saldo;
                    // resetear fecha de pago
                    cuota.FechaPago = null;
                    cuota.EstadoCuota = Cuota.Estado.Pendiente;

                    _context.Update(cuota);
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Solicitudes", new { id = solicitudId });
        }




        // =========================
        // DETALLE UNITARIO (renombrado para no chocar con Details)
        // GET: /Cuotas/DetalleCuota/5?solicitudId=123
        // =========================
        [HttpGet]
        public async Task<IActionResult> DetalleCuota(int? id, int? solicitudId)
        {
            if (id == null) return NotFound();

            var cuota = await _context.Cuotas
                .Include(c => c.Solicitud)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.CuotaId == id);

            if (cuota == null) return NotFound();

            ViewBag.SolicitudId = solicitudId ?? cuota.SolicitudId;
            return View(cuota); // -> Views/Cuotas/DetalleCuota.cshtml (Cuota)
        }

        // ====== Create/Edit/Delete (SIN CAMBIOS sustanciales) ======

        public IActionResult Create(int? solicitudId)
        {
            if (solicitudId.HasValue)
                ViewBag.SolicitudId = solicitudId.Value;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CuotaId,SolicitudId,Numerocuota,MontoCuota,SaldoCuota,FechaVencimiento,FechaPago")] Cuota cuota)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.SolicitudId = cuota.SolicitudId;
                return View(cuota);
            }

            _context.Add(cuota);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { solicitudId = cuota.SolicitudId });
        }

        public async Task<IActionResult> Edit(int? id, int? solicitudId)
        {
            if (id == null) return NotFound();

            var cuota = await _context.Cuotas.FindAsync(id);
            if (cuota == null) return NotFound();

            ViewBag.SolicitudId = solicitudId ?? cuota.SolicitudId;
            return View(cuota);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CuotaId,SolicitudId,Numerocuota,MontoCuota,SaldoCuota,FechaVencimiento,FechaPago")] Cuota cuota)
        {
            if (id != cuota.CuotaId) return NotFound();

            try
            {
                _context.Update(cuota);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _context.Cuotas.AnyAsync(e => e.CuotaId == cuota.CuotaId);
                if (!exists) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Details), new { solicitudId = cuota.SolicitudId });
        }

        public async Task<IActionResult> Delete(int? id, int? solicitudId)
        {
            if (id == null) return NotFound();

            var cuota = await _context.Cuotas
                .Include(c => c.Solicitud)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.CuotaId == id);

            if (cuota == null) return NotFound();

            ViewBag.SolicitudId = solicitudId ?? cuota.SolicitudId;
            return View(cuota);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cuota = await _context.Cuotas.FindAsync(id);
            int? solicitudId = cuota?.SolicitudId;

            if (cuota != null)
                _context.Cuotas.Remove(cuota);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { solicitudId });
        }
    }
}
