using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using xeepconcesionario.Data;
using xeepconcesionario.Models;
using xeepconcesionario.Services;

namespace xeepconcesionario.Controllers
{
    public class CobrosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IReceiptPdfService _pdf;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ActividadSolicitudService _actividadService;
        public CobrosController(ApplicationDbContext context, IReceiptPdfService pdf, UserManager<ApplicationUser> userManager, ActividadSolicitudService actividadService)
        {
            _context = context;
            _pdf = pdf;
            _userManager = userManager;
            _actividadService = actividadService;
        }
        public async Task<IActionResult> Index(
            int? solicitudId,
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            int? numerocuota,
            string? nombresupervisor)
        {
            // Normalizo fechas (hasta inclusivo todo el día)
            var desde = (fechaDesde ?? DateTime.Today).Date;
            var hasta = (fechaHasta ?? DateTime.Today).Date;
            var hastaExclusivo = hasta.AddDays(1);

            // Base query
            var query = _context.Cobros
                .Include(c => c.Usuario)
                .Include(c => c.Solicitud).ThenInclude(s => s.Cliente)
                .Include(c => c.Cuota)
                .Include(c => c.Solicitud.Supervisor)
                .AsNoTracking()
                .Where(c => c.Fecha >= desde && c.Fecha < hastaExclusivo);

            // 🔹 Filtro por solicitud específica
            if (solicitudId.HasValue)
            {
                query = query.Where(c => c.SolicitudId == solicitudId.Value);
                ViewBag.SolicitudId = solicitudId.Value;

                ViewBag.Solicitud = await _context.Solicitudes
                    .Include(s => s.Cliente)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.SolicitudId == solicitudId.Value);
            }

            // 🔹 Filtro por número de cuota
            if (numerocuota.HasValue)
                query = query.Where(c => c.Cuota != null && c.Cuota.Numerocuota == numerocuota.Value);

            // 🔹 Filtro por nombre de supervisor (contiene, case-insensitive)
            if (!string.IsNullOrWhiteSpace(nombresupervisor))
                query = query.Where(c =>
                    c.Solicitud.Supervisor != null &&
                    c.Solicitud.Supervisor.NombreCompleto != null &&
                    EF.Functions.ILike(c.Solicitud.Supervisor.NombreCompleto, $"%{nombresupervisor}%")
                );

            // Pasar valores actuales para inputs
            ViewBag.FechaDesde = desde.ToString("yyyy-MM-dd");
            ViewBag.FechaHasta = hasta.ToString("yyyy-MM-dd");
            ViewBag.NumeroCuota = numerocuota;
            ViewBag.NombreSupervisor = nombresupervisor;

            // Ejecutar consulta
            var lista = await query
                .OrderByDescending(c => c.Fecha)
                .ToListAsync();

            return View(lista);
        }




        // GET: Cobros/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cobro = await _context.Cobros
                .Include(c => c.Cuota)
                .Include(c => c.Solicitud)
                .FirstOrDefaultAsync(m => m.CobroId == id);
            if (cobro == null)
            {
                return NotFound();
            }

            return View(cobro);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cobro model)
        {
            // 1) Guardar cobro y actualizar cuota
            model.UsuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _context.Cobros.Add(model);

            var cuota = await _context.Cuotas.FirstOrDefaultAsync(c => c.CuotaId == model.CuotaId);
            if (cuota != null)
            {
                cuota.SaldoCuota = Math.Max(0, cuota.SaldoCuota - model.Monto);
                if (cuota.SaldoCuota == 0)
                    cuota.FechaPago = model.Fecha.Date;

                _context.Cuotas.Update(cuota);
            }

            await _context.SaveChangesAsync();

            // 2) Cambiar estado de la solicitud si corresponde
            var solicitud = await _context.Solicitudes
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.SolicitudId == model.SolicitudId);

            if (solicitud?.Plan != null)
            {
                int cantApertura = solicitud.Plan.CuotaApertura;
                if (cantApertura > 0)
                {
                    int pendientesApertura = await _context.Cuotas
                        .Where(c => c.SolicitudId == solicitud.SolicitudId
                                    && c.Numerocuota >= 1
                                    && c.Numerocuota <= cantApertura
                                    && c.SaldoCuota > 0)
                        .CountAsync();

                    if (pendientesApertura == 0)
                    {
                        solicitud.EstadoId = 2; // tu ID/enum de "abierta"/"activa"
                        _context.Solicitudes.Update(solicitud);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            // 3) Registrar la actividad automáticamente
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _actividadService.RegistrarActividadAsync(
                solicitudId: model.SolicitudId,
                estadoActividadId: 2, // Por ejemplo: 2 = "Pago de cuota"
                observacion: $"Cobro de cuota #{cuota?.Numerocuota} por {model.Monto:C}. {model.ObservacionCobro}",
                usuarioId: userId
            );

            // En vez de generar y devolver el PDF acá...
            var urlRecibo = Url.Action("ReciboPorCuota", "Cobros", new { cuotaId = model.CuotaId });
            var urlVolver = Url.Action("Details", "Cuotas", new { solicitudId = model.SolicitudId });

            var html = $@"
                <!doctype html>
                <html><head><meta charset='utf-8'></head>
                <body>
                <script>
                  // Abre el recibo en otra pestaña
                  window.open('{urlRecibo}', '_blank');
                  // Y vuelve a la lista de cuotas
                  window.location = '{urlVolver}';
                </script>
                </body></html>";

            return Content(html, "text/html");

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CobrarPorMonto(int solicitudId, decimal monto, DateTime? fecha)
        {
            if (monto <= 0)
            {
                TempData["Error"] = "El monto debe ser mayor a cero.";
                return RedirectToAction("Details", "Cuotas", new { solicitudId });
            }

            var fechaCobro = (fecha ?? DateTime.Today).Date;

            await using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                // Traigo las cuotas pendientes de la solicitud, en orden
                var cuotas = await _context.Cuotas
                    .Where(c => c.SolicitudId == solicitudId && c.SaldoCuota > 0)
                    .OrderBy(c => c.Numerocuota)
                    .ToListAsync();

                if (cuotas.Count == 0)
                {
                    TempData["Info"] = "No hay cuotas con saldo pendiente para esta solicitud.";
                    return RedirectToAction("Details", "Cuotas", new { solicitudId });
                }

                decimal restante = monto;
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                foreach (var cuota in cuotas)
                {
                    if (restante <= 0) break;

                    var aPagar = Math.Min(cuota.SaldoCuota, restante);
                    if (aPagar <= 0) continue;

                    // Registro un cobro por esta cuota (monto parcial o total)
                    var cobro = new Cobro
                    {
                        SolicitudId = solicitudId,
                        CuotaId = cuota.CuotaId,
                        Fecha = fechaCobro,         // sin tocar Kind
                        Monto = aPagar,
                        UsuarioId = userId
                    };
                    _context.Cobros.Add(cobro);

                    // Actualizo saldo y fecha de pago si quedó en cero
                    cuota.SaldoCuota -= aPagar;
                    if (cuota.SaldoCuota <= 0)
                    {
                        cuota.SaldoCuota = 0;
                        cuota.FechaPago = fechaCobro; // date "puro"
                    }
                    _context.Cuotas.Update(cuota);

                    restante -= aPagar;
                }

                await _context.SaveChangesAsync();

                // --- Misma lógica de estado al completar apertura ---
                var solicitud = await _context.Solicitudes
                    .Include(s => s.Plan)
                    .FirstOrDefaultAsync(s => s.SolicitudId == solicitudId);

                if (solicitud?.Plan != null)
                {
                    int cantApertura = solicitud.Plan.CuotaApertura;
                    if (cantApertura > 0)
                    {
                        int pendientesApertura = await _context.Cuotas
                            .Where(c => c.SolicitudId == solicitudId
                                        && c.Numerocuota >= 1
                                        && c.Numerocuota <= cantApertura
                                        && c.SaldoCuota > 0)
                            .CountAsync();

                        if (pendientesApertura == 0)
                        {
                            solicitud.EstadoId = 2; // o tu enum/valor correcto
                            _context.Solicitudes.Update(solicitud);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                // ----------------------------------------------------

                // 3) Registrar la actividad de cobro (resumen)
                var cuotasPagadas = cuotas
                    .Where(c => c.FechaPago == fechaCobro) // cuotas que se tocaron en este cobro
                    .Select(c => c.Numerocuota)
                    .ToList();

                if (cuotasPagadas.Any())
                {
                    var detalle = string.Join(", ", cuotasPagadas);

                    await _actividadService.RegistrarActividadAsync(
                        solicitudId: solicitudId,
                        estadoActividadId: 2, // "Pago de cuota"
                        observacion: $"Cobro por monto {monto:C} aplicado a cuotas: {detalle}.",
                        usuarioId: userId!
                    );
                }


                await tx.CommitAsync();

                if (restante > 0)
                    TempData["Info"] = $"Se cancelaron todas las cuotas pendientes. Sobró: {restante:C}.";

                return RedirectToAction("Details", "Cuotas", new { solicitudId });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                TempData["Error"] = $"No se pudo procesar el cobro: {ex.Message}";
                return RedirectToAction("Details", "Cuotas", new { solicitudId });
            }
        }


        // GET: Cobros/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cobro = await _context.Cobros.FindAsync(id);
            if (cobro == null)
            {
                return NotFound();
            }
            ViewData["CuotaId"] = new SelectList(_context.Cuotas, "CuotaId", "CuotaId", cobro.CuotaId);
            ViewData["SolicitudId"] = new SelectList(_context.Solicitudes, "SolicitudId", "SolicitudId", cobro.SolicitudId);
            return View(cobro);
        }

        // POST: Cobros/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CobroId,SolicitudId,CobradorId,UsuarioId,CuotaId,Fecha,Monto")] Cobro cobro)
        {
            if (id != cobro.CobroId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cobro);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CobroExists(cobro.CobroId))
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
            ViewData["CuotaId"] = new SelectList(_context.Cuotas, "CuotaId", "CuotaId", cobro.CuotaId);
            ViewData["SolicitudId"] = new SelectList(_context.Solicitudes, "SolicitudId", "SolicitudId", cobro.SolicitudId);
            return View(cobro);
        }

        // GET: Cobros/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cobro = await _context.Cobros
                .Include(c => c.Cuota)
                .Include(c => c.Solicitud)
                .FirstOrDefaultAsync(m => m.CobroId == id);
            if (cobro == null)
            {
                return NotFound();
            }

            return View(cobro);
        }

        [HttpGet]
        public async Task<IActionResult> ReciboPorCuota(int cuotaId)
        {
            var cobro = await _context.Cobros
                .Include(c => c.Solicitud).ThenInclude(s => s.Cliente)
                .FirstOrDefaultAsync(c => c.CuotaId == cuotaId);

            if (cobro == null)
                return NotFound();

            var saldoActual = await _context.Cuotas
                .Where(q => q.SolicitudId == cobro.SolicitudId)
                .SumAsync(q => q.SaldoCuota);

            var saldoAnterior = saldoActual + cobro.Monto;

            var user = await _userManager.GetUserAsync(User);

            var dto = new ReciboCobroDto(
                CobroId: cobro.CobroId,
                Fecha: DateTime.Now,
                Cliente: cobro.Solicitud?.Cliente?.ApellidoYNombre ?? "—",
                DocumentoCliente: cobro.Solicitud?.Cliente?.Dni?.ToString(),
                SolicitudInfo: $"#{cobro.Solicitud?.NumeroSolicitud}",
                Importe: cobro.Monto,
                SaldoAnterior: saldoAnterior,
                SaldoActual: saldoActual,
                Observacion: cobro.ObservacionCobro,
                Usuario: user?.NombreCompleto ?? "—"
            );


            byte[]? logo = null;
            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo.png");
            if (System.IO.File.Exists(logoPath))
                logo = await System.IO.File.ReadAllBytesAsync(logoPath);

            var pdf = _pdf.Generar(dto, logo);
            Response.Headers["Content-Disposition"] = $"inline; filename=Recibo_{cobro.CobroId}.pdf";
            return File(pdf, "application/pdf");
        }

        [HttpGet]
        public async Task<IActionResult> ExportarExcel(DateTime? fechaDesde, DateTime? fechaHasta, int? numeroCuota, string? nombreSupervisor)
        {
            var query = _context.Cobros
                .Include(c => c.Solicitud)
                    .ThenInclude(s => s.Cliente)
                .Include(c => c.Solicitud)
                    .ThenInclude(s => s.Supervisor)
                .Include(c => c.Cuota)
                .Include(c => c.Usuario)
                .AsQueryable();

            if (fechaDesde.HasValue)
                query = query.Where(c => c.Fecha >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(c => c.Fecha <= fechaHasta.Value);

            if (numeroCuota.HasValue)
                query = query.Where(c => c.Cuota.Numerocuota == numeroCuota.Value);

            if (!string.IsNullOrEmpty(nombreSupervisor))
                query = query.Where(c => c.Solicitud.Supervisor.NombreCompleto.Contains(nombreSupervisor));

            var cobros = await query.OrderBy(c => c.Fecha).ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Cobros");

            // ---- Encabezados ----
            string[] headers = { "Fecha", "Monto", "Solicitud", "Cliente", "Cuota", "Supervisor", "Usuario" };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            }

            // ---- Filas de datos ----
            int row = 2;
            foreach (var c in cobros)
            {
                ws.Cell(row, 1).Value = c.Fecha.ToString("dd/MM/yyyy");
                ws.Cell(row, 2).Value = c.Monto;
                ws.Cell(row, 2).Style.NumberFormat.Format = "$ #,##0.00"; // formato moneda
                ws.Cell(row, 3).Value = c.Solicitud?.NumeroSolicitud ?? 0;
                ws.Cell(row, 4).Value = c.Solicitud?.Cliente?.ApellidoYNombre ?? "-";
                ws.Cell(row, 5).Value = c.Cuota != null ? $"N° {c.Cuota.Numerocuota} ({c.Cuota.CuotaId})" : "-";
                ws.Cell(row, 6).Value = c.Solicitud?.Supervisor?.NombreCompleto ?? "-";
                ws.Cell(row, 7).Value = c.Usuario?.NombreCompleto ?? c.Usuario?.UserName ?? c.Usuario?.Email ?? "-";
                row++;
            }

            // ---- Totales ----
            decimal totalMonto = cobros.Sum(c => c.Monto);
            int totalRegistros = cobros.Count;

            var totalRow = row + 1;
            ws.Cell(totalRow, 1).Value = "Totales:";
            ws.Cell(totalRow, 1).Style.Font.Bold = true;

            ws.Cell(totalRow, 2).Value = totalMonto;
            ws.Cell(totalRow, 2).Style.NumberFormat.Format = "$ #,##0.00";
            ws.Cell(totalRow, 2).Style.Font.Bold = true;
            ws.Cell(totalRow, 2).Style.Fill.BackgroundColor = XLColor.LightYellow;

            ws.Cell(totalRow, 3).Value = $"Registros: {totalRegistros}";
            ws.Range(totalRow, 3, totalRow, 7).Merge();
            ws.Cell(totalRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(totalRow, 3).Style.Font.Bold = true;
            ws.Cell(totalRow, 3).Style.Fill.BackgroundColor = XLColor.LightYellow;

            // Ajustar tamaño de columnas
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Cobros_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }


        // POST: Cobros/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cobro = await _context.Cobros.FindAsync(id);
            if (cobro != null)
            {
                _context.Cobros.Remove(cobro);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CobroExists(int id)
        {
            return _context.Cobros.Any(e => e.CobroId == id);
        }
    }
}
