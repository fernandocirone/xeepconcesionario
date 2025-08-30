using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace xeepconcesionario.Models
{
    public class ActividadSolicitud
    {
        public int ActividadSolicitudId { get; set; }

        // Relación con Solicitud
        [Required]
        public int SolicitudId { get; set; }
        public Solicitud Solicitud { get; set; } = null!;

        // Relación con EstadoActividad (catálogo)
        [Required]
        public int EstadoActividadId { get; set; }
        public EstadoActividad EstadoActividad { get; set; } = null!;

        // Fecha de la actividad
        [Column(TypeName = "timestamp without time zone")]
        public DateTime Fecha { get; set; } = DateTime.Now;

        // Texto largo de observación
        [DataType(DataType.MultilineText)]
        public string? Observacion { get; set; }

        // Usuario que registró la actividad
        [Required]
        public string UsuarioId { get; set; } = null!;
        public ApplicationUser Usuario { get; set; } = null!;
    }
}
