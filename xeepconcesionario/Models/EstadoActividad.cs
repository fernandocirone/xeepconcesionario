namespace xeepconcesionario.Models
{
    public class EstadoActividad
    {
        public int EstadoActividadId { get; set; }
        public string NombreEstadoActividad { get; set; } = string.Empty;

        public ICollection<ActividadSolicitud> Actividades { get; set; } = new List<ActividadSolicitud>();
    }
}
