using xeepconcesionario.Data;
using xeepconcesionario.Models;


namespace xeepconcesionario.Services
{
    public class ActividadSolicitudService
    {
        private readonly ApplicationDbContext _context;

        public ActividadSolicitudService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task RegistrarActividadAsync(
            int solicitudId,
            int estadoActividadId,
            string observacion,
            string usuarioId)
        {
            var actividad = new ActividadSolicitud
            {
                SolicitudId = solicitudId,
                EstadoActividadId = estadoActividadId,
                Observacion = observacion,
                Fecha = DateTime.Now,
                UsuarioId = usuarioId
            };

            _context.ActividadesSolicitud.Add(actividad);
            await _context.SaveChangesAsync();
        }
    }
}
