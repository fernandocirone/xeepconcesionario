using System;
using System.Collections.Generic;
using System.IO;
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

namespace xeepconcesionario.Controllers
{
    public class SolicitudesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SolicitudesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =============== INDEX ===============
        public async Task<IActionResult> Index(
            string? clienteNombre,
            string? estado,
            string? vendedorNombre,
            string? supervisorNombre,
            string? jefeVentasNombre,
            int? estadoId,
            int? condicionVentaId,
            int? tipoBajaId,
            string? PlanTexto,
            DateTime? fechaCargaDesde,
            DateTime? fechaCargaHasta,
            string? dni,
            string? telefono,
            string? provincia,
            string? localidad,
            string? region,
            string? numeroSolicitud)
        {
            // Query con todos los filtros unificados
            var q = BuildQuery(clienteNombre, estado, vendedorNombre, supervisorNombre, jefeVentasNombre,
                               estadoId, condicionVentaId, tipoBajaId, PlanTexto,
                               fechaCargaDesde, fechaCargaHasta, dni, telefono,
                               provincia, localidad, region, numeroSolicitud)
                    .AsNoTracking();

            // Combos rápidos de la vista de listado (si los usás)
            ViewBag.ContratoId = new SelectList(await _context.Contratos.AsNoTracking().OrderBy(x => x.NombreContrato).ToListAsync(), "ContratoId", "NombreContrato");
            ViewBag.ClienteId = new SelectList(await _context.Clientes.AsNoTracking().OrderBy(c => c.ApellidoYNombre).ToListAsync(), "ClienteId", "ApellidoYNombre");
            ViewBag.PlanId = new SelectList(await _context.Planes.AsNoTracking().OrderBy(a => a.Modelo).ToListAsync(), "PlanId", "Modelo");
            ViewBag.CondicionVentaId = new SelectList(await _context.CondicionesVenta.AsNoTracking().OrderBy(c => c.NombreCondicionVenta).ToListAsync(), "CondicionVentaId", "NombreCondicionVenta", condicionVentaId);
            ViewBag.TipoBajaId = new SelectList(await _context.TiposBaja.AsNoTracking().OrderBy(t => t.NombreTipoBaja).ToListAsync(), "TipoBajaId", "NombreTipoBaja", tipoBajaId);
            ViewBag.EstadoId = new SelectList(await _context.Estados.AsNoTracking().OrderBy(e => e.NombreEstado).ToListAsync(), "EstadoId", "NombreEstado", estadoId);

            // Guardar valores de filtros (para que queden en la UI)
            ViewBag.ClienteNombre = clienteNombre;
            ViewBag.EstadoNombre = estado;
            ViewBag.VendedorNombre = vendedorNombre;
            ViewBag.SupervisorNombre = supervisorNombre;
            ViewBag.JefeVentasNombre = jefeVentasNombre;
            ViewBag.PlanTexto = PlanTexto;
            ViewBag.FechaCargaDesde = fechaCargaDesde;
            ViewBag.FechaCargaHasta = fechaCargaHasta;
            ViewBag.Dni = dni;
            ViewBag.Telefono = telefono;
            ViewBag.Provincia = provincia;
            ViewBag.Localidad = localidad;
            ViewBag.Region = region;
            ViewBag.NumeroSolicitud = numeroSolicitud;

            var lista = await q
                .OrderByDescending(s => s.FechaCarga)
                .ThenBy(s => s.SolicitudId)
                .ToListAsync();

            return View(lista);
        }

        // =============== SELLADOS ===============
        [HttpGet("sellados/{id:int}")]
        public async Task<IActionResult> Sellados(int id)
        {
            var auto = await _context.Planes
                .AsNoTracking()
                .Where(a => a.PlanId == id)
                .Select(a => new { sellado1 = a.Sellado, sellado2 = a.Sellado })
                .FirstOrDefaultAsync();

            if (auto is null) return NotFound();
            return Ok(auto);
        }

        // =============== HELPERS DE COMBOS (USUARIOS) ===============
        private async Task<SelectList> GetUsuariosPorTipoAsync(int tipo, string? seleccionadoId = null)
        {
            var data = await _userManager.Users
                .Where(u => u.TiposUsuario.Any(tu => tu.TipoUsuarioId == tipo))
                .Select(u => new { u.Id, Texto = u.NombreCompleto ?? u.UserName })
                .OrderBy(x => x.Texto)
                .ToListAsync();

            return new SelectList(data, "Id", "Texto", seleccionadoId);
        }

        private async Task RellenarCombosCreateAsync(
            string? vendedorIdSel = null,
            string? supervisorIdSel = null,
            string? jefeIdSel = null)
        {
            // Autos
            var autos = await _context.Planes
                .AsNoTracking()
                .Select(a => new { a.PlanId, Texto = a.Codigo + " - " + a.Modelo })
                .OrderBy(a => a.Texto)
                .ToListAsync();
            ViewBag.PlanId = new SelectList(autos, "PlanId", "Texto");

            ViewBag.ClienteId = new SelectList(
                await _context.Clientes.AsNoTracking().OrderBy(c => c.ApellidoYNombre).ToListAsync(),
                "ClienteId", "ApellidoYNombre");

            ViewBag.CondicionVentaId = new SelectList(
                await _context.CondicionesVenta.AsNoTracking().OrderBy(x => x.NombreCondicionVenta).ToListAsync(),
                "CondicionVentaId", "NombreCondicionVenta");

            ViewBag.EstadoId = new SelectList(
                await _context.Estados.AsNoTracking().OrderBy(x => x.NombreEstado).ToListAsync(),
                "EstadoId", "NombreEstado");

            ViewBag.TipoBajaId = new SelectList(
                await _context.TiposBaja.AsNoTracking().OrderBy(x => x.NombreTipoBaja).ToListAsync(),
                "TipoBajaId", "NombreTipoBaja");

            ViewBag.ContratoId = new SelectList(
                await _context.Contratos.AsNoTracking().OrderBy(x => x.NombreContrato).ToListAsync(),
                "ContratoId", "NombreContrato");

            // Usuarios por rol
            ViewBag.VendedorUserId = await GetUsuariosPorTipoAsync(1, vendedorIdSel);
            ViewBag.SupervisorUserId = await GetUsuariosPorTipoAsync(2, supervisorIdSel);
            ViewBag.JefeVentasUserId = await GetUsuariosPorTipoAsync(3, jefeIdSel);

            // Bloqueo del vendedor si el logueado es vendedor
            var user = await _userManager.GetUserAsync(User);
            ViewBag.VendedorBloqueado =
                (user?.TiposUsuario?.Any(tu => tu.TipoUsuarioId == 1) ?? false)
                && !string.IsNullOrEmpty(vendedorIdSel);

            // Localidades para ClienteNuevo.LocalidadId
            var localidades = await _context.Localidades
                .AsNoTracking()
                .OrderBy(l => l.NombreLocalidad)
                .ToListAsync();

            ViewBag.LocalidadId = new SelectList(localidades, "LocalidadId", "NombreLocalidad");
        }

        private async Task RellenarCombosAsync(
            Solicitud? s = null,
            string? vendedorIdForzado = null,
            bool bloquearVendedor = false)
        {
            // Localidades
            var localidades = await _context.Localidades
                .AsNoTracking()
                .OrderBy(l => l.NombreLocalidad)
                .Select(l => new { l.LocalidadId, Texto = l.NombreLocalidad })
                .ToListAsync();
            ViewBag.LocalidadId = new SelectList(localidades, "LocalidadId", "Texto");

            // Autos
            var autos = await _context.Planes
                .AsNoTracking()
                .Select(a => new { a.PlanId, Texto = a.Codigo + " - " + a.Modelo })
                .OrderBy(a => a.Texto)
                .ToListAsync();
            ViewBag.PlanId = new SelectList(autos, "PlanId", "Texto", s?.PlanId);

            // Entidades varias
            ViewBag.ClienteId = new SelectList(
                await _context.Clientes.AsNoTracking().OrderBy(c => c.ApellidoYNombre).ToListAsync(),
                "ClienteId", "ApellidoYNombre", s?.ClienteId);

            ViewBag.CondicionVentaId = new SelectList(
                await _context.CondicionesVenta.AsNoTracking().OrderBy(x => x.NombreCondicionVenta).ToListAsync(),
                "CondicionVentaId", "NombreCondicionVenta", s?.CondicionVentaId);

            ViewBag.EstadoId = new SelectList(
                await _context.Estados.AsNoTracking().OrderBy(x => x.NombreEstado).ToListAsync(),
                "EstadoId", "NombreEstado", s?.EstadoId);

            ViewBag.TipoBajaId = new SelectList(
                await _context.TiposBaja.AsNoTracking().OrderBy(x => x.NombreTipoBaja).ToListAsync(),
                "TipoBajaId", "NombreTipoBaja", s?.TipoBajaId);

            ViewBag.ContratoId = new SelectList(
                await _context.Contratos.AsNoTracking().OrderBy(x => x.NombreContrato).ToListAsync(),
                "ContratoId", "NombreContrato", s?.ContratoId);

            // Usuarios (con vendedor preforzado si corresponde)
            var vendedorSel = vendedorIdForzado ?? s?.VendedorUserId;
            var supervisorSel = s?.SupervisorUserId;
            var jefeSel = s?.JefeVentasUserId;

            ViewBag.VendedorUserId = await GetUsuariosPorTipoAsync(1, vendedorSel);
            ViewBag.SupervisorUserId = await GetUsuariosPorTipoAsync(2, supervisorSel);
            ViewBag.JefeVentasUserId = await GetUsuariosPorTipoAsync(3, jefeSel);

            ViewBag.VendedorBloqueado = bloquearVendedor;
        }

        // =============== EDIT (GET/POST) ===============
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var solicitud = await _context.Solicitudes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.SolicitudId == id);
            if (solicitud == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);

            await RellenarCombosAsync(
                s: solicitud,
                vendedorIdForzado: user?.TiposUsuario?.Any(tu => tu.TipoUsuarioId == 1) == true ? user.Id : null,
                bloquearVendedor: true
            );

            return View(solicitud);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
           int id,
           [Bind("SolicitudId,NumeroSolicitud,ContratoId,VendedorUserId,SupervisorUserId,JefeVentasUserId,UsuarioId,ClienteId,PlanId,CondicionVentaId,TipoBajaId,EstadoId,FechaCarga,FechaSuscripcion,ValorSellado1,ValorSellado2,CantidadCuotas,FechaPrimerVencimiento,ImporteCuota,ObservacionSolicitud")]
           Solicitud solicitud)
        {
            if (id != solicitud.SolicitudId) return NotFound();

            // Forzar vendedor si el logueado es vendedor
            var user = await _userManager.GetUserAsync(User);
            if (user?.TiposUsuario?.Any(tu => tu.TipoUsuarioId == 1) == true)
                solicitud.VendedorUserId = user.Id;

            static DateTime AsUnspecified(DateTime dt) =>
                DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);

            if (solicitud.FechaCarga.HasValue)
                solicitud.FechaCarga = AsUnspecified(solicitud.FechaCarga.Value);
            if (solicitud.FechaSuscripcion.HasValue)
                solicitud.FechaSuscripcion = AsUnspecified(solicitud.FechaSuscripcion.Value);

            try
            {
                _context.Update(solicitud);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                bool exists = await _context.Solicitudes.AsNoTracking().AnyAsync(s => s.SolicitudId == solicitud.SolicitudId);
                if (!exists) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // =============== CREATE (GET/POST) ===============
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);

            // Si es vendedor, lo preseleccionamos y bloqueamos
            string? vendedorSel = user?.TiposUsuario?.Any(tu => tu.TipoUsuarioId == 1) == true ? user.Id : null;

            await RellenarCombosCreateAsync(
                vendedorIdSel: vendedorSel,
                supervisorIdSel: null,
                jefeIdSel: null
            );

            var vm = new CreateSolicitudViewModel
            {
                EstadoId = 1,
                FechaCarga = DateTime.Today,
                VendedorUserId = vendedorSel
            };

            ViewBag.VendedorBloqueado =
                (user?.TiposUsuario?.Any(tu => tu.TipoUsuarioId == 1) == true)
                && !string.IsNullOrEmpty(vendedorSel);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSolicitudViewModel vm)
        {
            static DateTime AsUnspecified(DateTime dt) =>
                DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);

            var user = await _userManager.GetUserAsync(User);

            // Forzar vendedor si el logueado es vendedor
            if (user?.TiposUsuario?.Any(tu => tu.TipoUsuarioId == 1) == true)
                vm.VendedorUserId = user.Id;

            // Usuario que carga
            vm.UsuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Validaciones mínimas
            if (vm.PlanId == 0)
                ModelState.AddModelError(nameof(vm.PlanId), "Debe seleccionar un automóvil.");

            if (!vm.CrearClienteNuevo && (!vm.ClienteId.HasValue || vm.ClienteId.Value <= 0))
                ModelState.AddModelError(nameof(vm.ClienteId), "Debe seleccionar un cliente o cargar uno nuevo.");

            if (!ModelState.IsValid)
            {
                await CargarCombosCreate(vm);
                return View(vm);
            }

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1) Cliente (nuevo o existente)
                int clienteId;
                if (vm.CrearClienteNuevo)
                {
                    var cli = vm.ClienteNuevo ?? new Cliente();
                    _context.Clientes.Add(cli);
                    await _context.SaveChangesAsync();
                    clienteId = cli.ClienteId;
                }
                else
                {
                    clienteId = vm.ClienteId!.Value;
                }

                // 2) Fechas normalizadas
                if (vm.FechaCarga.HasValue) vm.FechaCarga = AsUnspecified(vm.FechaCarga.Value);
                if (vm.FechaSuscripcion.HasValue) vm.FechaSuscripcion = AsUnspecified(vm.FechaSuscripcion.Value);

                // 3) Crear Solicitud
                var solicitud = new Solicitud
                {
                    ContratoId = vm.ContratoId,
                    VendedorUserId = vm.VendedorUserId,
                    SupervisorUserId = vm.SupervisorUserId,
                    JefeVentasUserId = vm.JefeVentasUserId,
                    UsuarioId = vm.UsuarioId!,
                    ClienteId = clienteId,
                    PlanId = vm.PlanId,
                    CondicionVentaId = vm.CondicionVentaId,
                    TipoBajaId = vm.TipoBajaId,
                    EstadoId = vm.EstadoId <= 0 ? 1 : vm.EstadoId,
                    FechaCarga = vm.FechaCarga,
                    FechaSuscripcion = vm.FechaSuscripcion,
                    ValorSellado1 = vm.ValorSellado1,
                    ValorSellado2 = vm.ValorSellado2,
                    NumeroSolicitud = vm.NumeroSolicitud,
                    ObservacionSolicitud = vm.ObservacionSolicitud
                };

                _context.Solicitudes.Add(solicitud);
                await _context.SaveChangesAsync();

                // 4) Importe de cuota
                var auto = await _context.Planes.AsNoTracking()
                    .FirstOrDefaultAsync(a => a.PlanId == vm.PlanId)
                    ?? throw new InvalidOperationException("No se encontró el automóvil seleccionado.");

                decimal importe = vm.ImporteCuota ?? auto.AdelantoMensual;
                if (importe <= 0) throw new InvalidOperationException("El importe de la cuota calculado es inválido (<= 0).");

                // 5) Fecha base (día 10)
                DateTime baseDate = vm.FechaPrimerVencimiento ?? vm.FechaSuscripcion ?? DateTime.Now;
                DateTime fechaBase = baseDate.Day >= 10
                    ? new DateTime(baseDate.Year, baseDate.Month, 10).AddMonths(1)
                    : new DateTime(baseDate.Year, baseDate.Month, 10);
                fechaBase = AsUnspecified(fechaBase);

                // 6) Generar cuotas
                int cantidad = vm.CantidadCuotas <= 0 ? 99 : vm.CantidadCuotas;
                var cuotas = new List<Cuota>(capacity: cantidad);

                for (int n = 1; n <= cantidad; n++)
                {
                    var venc = AsUnspecified(new DateTime(fechaBase.Year, fechaBase.Month, 10).AddMonths(n - 1));
                    decimal monto = importe;
                    if (n == 1) monto += solicitud.ValorSellado1;
                    if (n == 2) monto += solicitud.ValorSellado2;

                    cuotas.Add(new Cuota
                    {
                        SolicitudId = solicitud.SolicitudId,
                        Numerocuota = n,
                        MontoCuota = monto,
                        SaldoCuota = monto,
                        FechaVencimiento = venc,
                        EstadoCuota = Cuota.Estado.Pendiente
                    });
                }

                _context.Cuotas.AddRange(cuotas);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        private async Task CargarCombosCreate(CreateSolicitudViewModel vm)
        {
            var autos = await _context.Planes
                .AsNoTracking()
                .Select(a => new { a.PlanId, Texto = a.Codigo + " - " + a.Modelo })
                .OrderBy(a => a.Texto)
                .ToListAsync();
            ViewBag.PlanId = new SelectList(autos, "PlanId", "Texto", vm.PlanId);

            var localidades = await _context.Localidades
                .AsNoTracking()
                .OrderBy(l => l.NombreLocalidad)
                .ToListAsync();

            ViewBag.LocalidadId = new SelectList(localidades, "LocalidadId", "NombreLocalidad", vm?.ClienteNuevo?.LocalidadId);

            ViewBag.ClienteId = new SelectList(
                await _context.Clientes.AsNoTracking().OrderBy(c => c.ApellidoYNombre).ToListAsync(),
                "ClienteId", "ApellidoYNombre", vm.ClienteId);

            ViewBag.CondicionVentaId = new SelectList(
                await _context.CondicionesVenta.AsNoTracking().OrderBy(x => x.NombreCondicionVenta).ToListAsync(),
                "CondicionVentaId", "NombreCondicionVenta", vm.CondicionVentaId);

            ViewBag.EstadoId = new SelectList(
                await _context.Estados.AsNoTracking().OrderBy(x => x.NombreEstado).ToListAsync(),
                "EstadoId", "NombreEstado", vm.EstadoId);

            ViewBag.TipoBajaId = new SelectList(
                await _context.TiposBaja.AsNoTracking().OrderBy(x => x.NombreTipoBaja).ToListAsync(),
                "TipoBajaId", "NombreTipoBaja", vm.TipoBajaId);

            ViewBag.ContratoId = new SelectList(
                await _context.Contratos.AsNoTracking().OrderBy(x => x.NombreContrato).ToListAsync(),
                "ContratoId", "NombreContrato", vm.ContratoId);

            // Usuarios por rol
            ViewBag.VendedorUserId = await GetUsuariosPorTipoAsync(1, vm.VendedorUserId);
            ViewBag.SupervisorUserId = await GetUsuariosPorTipoAsync(2, vm.SupervisorUserId);
            ViewBag.JefeVentasUserId = await GetUsuariosPorTipoAsync(3, vm.JefeVentasUserId);

            var user = await _userManager.GetUserAsync(User);
            ViewBag.VendedorBloqueado =
                (user?.TiposUsuario?.Any(tu => tu.TipoUsuarioId == 1) == true)
                && !string.IsNullOrEmpty(vm.VendedorUserId);
        }

        // =============== DELETE (GET/POST) ===============
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var solicitud = await _context.Solicitudes
                .Include(s => s.Cliente)
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(m => m.SolicitudId == id);

            if (solicitud == null) return NotFound();

            return View(solicitud);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var solicitud = await _context.Solicitudes.FindAsync(id);
            if (solicitud != null)
            {
                _context.Solicitudes.Remove(solicitud);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // =============== DETAILS ===============
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var solicitud = await _context.Solicitudes
                .Include(s => s.Cliente).ThenInclude(c => c.Localidad)
                .Include(s => s.Plan)
                .Include(s => s.CondicionVenta)
                .Include(s => s.TipoBaja)
                .Include(s => s.Estado)
                .Include(s => s.Vendedor)
                .Include(s => s.Supervisor)
                .Include(s => s.JefeVentas)
                .Include(s => s.Actividades).ThenInclude(a => a.EstadoActividad)
                .Include(s => s.Actividades).ThenInclude(a => a.Usuario)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SolicitudId == id);

            if (solicitud == null) return NotFound();

            var vm = new CreateSolicitudViewModel
            {
                SolicitudId = solicitud.SolicitudId,
                ContratoId = solicitud.ContratoId,
                NumeroSolicitud = solicitud.NumeroSolicitud,
                VendedorUserId = solicitud.VendedorUserId,
                SupervisorUserId = solicitud.SupervisorUserId,
                JefeVentasUserId = solicitud.JefeVentasUserId,
                UsuarioId = solicitud.UsuarioId,
                ClienteId = solicitud.ClienteId,
                CrearClienteNuevo = false,
                PlanId = solicitud.PlanId,
                CondicionVentaId = solicitud.CondicionVentaId,
                TipoBajaId = solicitud.TipoBajaId,
                EstadoId = solicitud.EstadoId,
                FechaCarga = solicitud.FechaCarga,
                FechaSuscripcion = solicitud.FechaSuscripcion,
                ValorSellado1 = solicitud.ValorSellado1,
                ValorSellado2 = solicitud.ValorSellado2,
                ObservacionSolicitud = solicitud.ObservacionSolicitud,
                Actividades = solicitud.Actividades.OrderByDescending(a => a.Fecha).ToList(),
                // nombres “amigables”
                PlanNombre = solicitud.Plan != null ? $"{solicitud.Plan.Codigo} - {solicitud.Plan.Modelo}" : null,
                CondicionVentaNombre = solicitud.CondicionVenta?.NombreCondicionVenta,
                TipoBajaNombre = solicitud.TipoBaja?.NombreTipoBaja,
                EstadoNombre = solicitud.Estado?.NombreEstado,
                VendedorNombre = solicitud.Vendedor?.NombreCompleto ?? solicitud.Vendedor?.UserName,
                SupervisorNombre = solicitud.Supervisor?.NombreCompleto ?? solicitud.Supervisor?.UserName,
                JefeVentasNombre = solicitud.JefeVentas?.NombreCompleto ?? solicitud.JefeVentas?.UserName,
                ClienteNombre = solicitud.Cliente?.ApellidoYNombre,
                LocalidadNombre = solicitud.Cliente?.Localidad?.NombreLocalidad,
                ContratoNombre = solicitud.ContratoId?.ToString()
            };

            if (solicitud.Cliente != null)
            {
                vm.ClienteNuevo = new Cliente
                {
                    ApellidoYNombre = solicitud.Cliente.ApellidoYNombre,
                    Dni = solicitud.Cliente.Dni,
                    TelefonoFijo = solicitud.Cliente.TelefonoFijo,
                    TelefonoCelular = solicitud.Cliente.TelefonoCelular,
                    Mail = solicitud.Cliente.Mail,
                    FechaNacimiento = solicitud.Cliente.FechaNacimiento,
                    Direccion = solicitud.Cliente.Direccion,
                    Nacionalidad = solicitud.Cliente.Nacionalidad,
                    LocalidadId = solicitud.Cliente.LocalidadId,
                    Barrio = solicitud.Cliente.Barrio,
                    TipoVivienda = solicitud.Cliente.TipoVivienda,
                    Sexo = solicitud.Cliente.Sexo,
                    EstadoCivil = solicitud.Cliente.EstadoCivil,
                    Ocupacion = solicitud.Cliente.Ocupacion,
                    Empresa = solicitud.Cliente.Empresa,
                    DomicilioLaboral = solicitud.Cliente.DomicilioLaboral,
                    Cargo = solicitud.Cliente.Cargo,
                    IngresosMensuales = solicitud.Cliente.IngresosMensuales,
                    TipoOcupacion = solicitud.Cliente.TipoOcupacion,
                    RazonSocial = solicitud.Cliente.RazonSocial,
                    TieneTarjetaCredito = solicitud.Cliente.TieneTarjetaCredito
                };
            }

            await CargarCombosCreate(vm);
            ViewBag.EstadosActividad = new SelectList(
                await _context.EstadosActividad.AsNoTracking().OrderBy(e => e.NombreEstadoActividad).ToListAsync(),
                "EstadoActividadId", "NombreEstadoActividad");

            return View(vm);
        }

        // =============== EXPORTAR EXCEL ===============
        [HttpGet]
        public async Task<IActionResult> ExportarExcel(
            string? clienteNombre,
            string? estado,
            string? vendedorNombre,
            string? supervisorNombre,
            string? jefeVentasNombre,
            int? estadoId,
            int? condicionVentaId,
            int? tipoBajaId,
            string? PlanTexto,
            DateTime? fechaCargaDesde,
            DateTime? fechaCargaHasta,
            string? dni,
            string? telefono,
            string? provincia,
            string? localidad,
            string? region,
            string? numeroSolicitud)
        {
            var solicitudes = await BuildQuery(clienteNombre, estado, vendedorNombre, supervisorNombre, jefeVentasNombre,
                                               estadoId, condicionVentaId, tipoBajaId, PlanTexto,
                                               fechaCargaDesde, fechaCargaHasta, dni, telefono,
                                               provincia, localidad, region, numeroSolicitud)
                                .AsNoTracking()
                                .OrderByDescending(s => s.FechaCarga)
                                .ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Solicitudes");

            string[] headers = {
                "Año", "ID Solicitud", "Cliente", "DNI", "Dirección",
                "Provincia", "Localidad", "Teléfono", "Vendedor", "Supervisor",
                "Jefe Ventas", "Región", "Fecha Carga", "Fecha Suscripción", "Cuotas Pagadas", "Estado"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            }

            int row = 2;
            foreach (var s in solicitudes)
            {
                ws.Cell(row, 1).Value = s.FechaCarga?.ToString("yyyy") ?? "";
                ws.Cell(row, 2).Value = s.NumeroSolicitud;
                ws.Cell(row, 3).Value = s.Cliente?.ApellidoYNombre ?? "—";
                ws.Cell(row, 4).Value = s.Cliente?.Dni ?? "—";
                ws.Cell(row, 5).Value = s.Cliente?.Direccion ?? "—";
                ws.Cell(row, 6).Value = s.Cliente?.Localidad?.Provincia?.NombreProvincia ?? "—";
                ws.Cell(row, 7).Value = s.Cliente?.Localidad?.NombreLocalidad ?? "—";
                ws.Cell(row, 8).Value = s.Cliente?.TelefonoCelular ?? "—";
                ws.Cell(row, 9).Value = s.Vendedor?.NombreCompleto ?? "—";
                ws.Cell(row, 10).Value = s.Supervisor?.NombreCompleto ?? "—";
                ws.Cell(row, 11).Value = s.JefeVentas?.NombreCompleto ?? "—";
                ws.Cell(row, 12).Value = s.Cliente?.Localidad?.Region?.NombreRegion ?? "—";
                ws.Cell(row, 13).Value = s.FechaCarga?.ToString("dd/MM/yyyy") ?? "";
                ws.Cell(row, 14).Value = s.FechaSuscripcion?.ToString("dd/MM/yyyy") ?? "";
                ws.Cell(row, 15).Value = s.Cuotas != null
                    ? $"{s.Cuotas.Count(c => c.Cobros != null && c.Cobros.Any())}"
                    : "0/0";
                ws.Cell(row, 16).Value = s.Estado?.NombreEstado ?? "—";
                row++;
            }

            int totalRegistros = solicitudes.Count;
            var totalRow = row + 1;

            ws.Cell(totalRow, 1).Value = "Totales:";
            ws.Cell(totalRow, 1).Style.Font.Bold = true;

            ws.Cell(totalRow, 2).Value = $"Registros: {totalRegistros}";
            ws.Range(totalRow, 2, totalRow, headers.Length).Merge();
            ws.Cell(totalRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(totalRow, 2).Style.Font.Bold = true;
            ws.Cell(totalRow, 2).Style.Fill.BackgroundColor = XLColor.LightYellow;

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Solicitudes_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        // =============== BUILD QUERY (filtros) ===============
        private IQueryable<Solicitud> BuildQuery(
            string? clienteNombre,
            string? estado,
            string? vendedorNombre,
            string? supervisorNombre,
            string? jefeVentasNombre,
            int? estadoId,
            int? condicionVentaId,
            int? tipoBajaId,
            string? PlanTexto,
            DateTime? fechaCargaDesde,
            DateTime? fechaCargaHasta,
            string? dni,
            string? telefono,
            string? provincia,
            string? localidad,
            string? region,
            string? numeroSolicitud)
        {
            var q = _context.Solicitudes
                .Include(s => s.Cuotas).ThenInclude(c => c.Cobros)
                .Include(s => s.Cliente).ThenInclude(c => c.Localidad).ThenInclude(l => l.Provincia)
                .Include(s => s.Cliente.Localidad.Region)
                .Include(s => s.Vendedor)
                .Include(s => s.Supervisor)
                .Include(s => s.JefeVentas)
                .Include(s => s.Estado)
                .Include(s => s.Plan)
                .Include(s => s.CondicionVenta)
                .Include(s => s.TipoBaja)
                .AsQueryable();

            // Texto -> ILIKE (insensible a mayúsculas) con nulos seguros
            if (!string.IsNullOrWhiteSpace(clienteNombre))
            {
                var like = $"%{clienteNombre.Trim()}%";
                q = q.Where(s => s.Cliente != null &&
                                 EF.Functions.ILike(s.Cliente.ApellidoYNombre!, like));
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                var like = $"%{estado.Trim()}%";
                q = q.Where(s => s.Estado != null &&
                                 EF.Functions.ILike(s.Estado.NombreEstado!, like));
            }

            if (!string.IsNullOrWhiteSpace(vendedorNombre))
            {
                var like = $"%{vendedorNombre.Trim()}%";
                q = q.Where(s => s.Vendedor != null &&
                                 EF.Functions.ILike(s.Vendedor.NombreCompleto!, like));
            }

            if (!string.IsNullOrWhiteSpace(supervisorNombre))
            {
                var like = $"%{supervisorNombre.Trim()}%";
                q = q.Where(s => s.Supervisor != null &&
                                 EF.Functions.ILike(s.Supervisor.NombreCompleto!, like));
            }

            if (!string.IsNullOrWhiteSpace(jefeVentasNombre))
            {
                var like = $"%{jefeVentasNombre.Trim()}%";
                q = q.Where(s => s.JefeVentas != null &&
                                 EF.Functions.ILike(s.JefeVentas.NombreCompleto!, like));
            }

            if (estadoId.HasValue)
                q = q.Where(s => s.EstadoId == estadoId.Value);

            if (condicionVentaId.HasValue)
                q = q.Where(s => s.CondicionVentaId == condicionVentaId.Value);

            if (tipoBajaId.HasValue)
                q = q.Where(s => s.TipoBajaId == tipoBajaId.Value);

            if (!string.IsNullOrWhiteSpace(PlanTexto))
            {
                var like = $"%{PlanTexto.Trim()}%";
                q = q.Where(s => s.Plan != null &&
                                 (EF.Functions.ILike(s.Plan.Modelo!, like) ||
                                  EF.Functions.ILike(s.Plan.Codigo!, like)));
            }

            // Fechas (rango cerrado por día)
            if (fechaCargaDesde.HasValue || fechaCargaHasta.HasValue)
            {
                var desde = (fechaCargaDesde ?? DateTime.MinValue).Date;
                var hasta = ((fechaCargaHasta ?? DateTime.MaxValue).Date).AddDays(1).AddTicks(-1);
                q = q.Where(s => s.FechaCarga >= desde && s.FechaCarga <= hasta);
            }

            // DNI y Teléfono: si están como string en DB, ILIKE; si no, Contains sobre ToString()
            if (!string.IsNullOrWhiteSpace(dni))
            {
                var like = $"%{dni.Trim()}%";
                q = q.Where(s => s.Cliente != null &&
                                ((s.Cliente.Dni != null && EF.Functions.ILike(s.Cliente.Dni, like)) ||
                                 (s.Cliente.Dni == null && false)));
            }

            if (!string.IsNullOrWhiteSpace(telefono))
            {
                var like = $"%{telefono.Trim()}%";
                q = q.Where(s => s.Cliente != null &&
                                 s.Cliente.TelefonoCelular != null &&
                                 EF.Functions.ILike(s.Cliente.TelefonoCelular, like));
            }

            if (!string.IsNullOrWhiteSpace(provincia))
            {
                var like = $"%{provincia.Trim()}%";
                q = q.Where(s => s.Cliente != null &&
                                 s.Cliente.Localidad != null &&
                                 s.Cliente.Localidad.Provincia != null &&
                                 EF.Functions.ILike(s.Cliente.Localidad.Provincia.NombreProvincia!, like));
            }

            if (!string.IsNullOrWhiteSpace(localidad))
            {
                var like = $"%{localidad.Trim()}%";
                q = q.Where(s => s.Cliente != null &&
                                 s.Cliente.Localidad != null &&
                                 EF.Functions.ILike(s.Cliente.Localidad.NombreLocalidad!, like));
            }

            if (!string.IsNullOrWhiteSpace(region))
            {
                var like = $"%{region.Trim()}%";
                q = q.Where(s => s.Cliente != null &&
                                 s.Cliente.Localidad != null &&
                                 s.Cliente.Localidad.Region != null &&
                                 EF.Functions.ILike(s.Cliente.Localidad.Region.NombreRegion!, like));
            }

            if (!string.IsNullOrWhiteSpace(numeroSolicitud))
            {
                if (int.TryParse(numeroSolicitud.Trim(), out var num))
                    q = q.Where(s => s.NumeroSolicitud == num);
                else
                {
                    // fallback: contiene (por si en DB es string)
                    var like = $"%{numeroSolicitud.Trim()}%";
                    q = q.Where(s => EF.Functions.ILike(s.NumeroSolicitud.ToString(), like));
                }
            }

            return q;
        }
    }
}
