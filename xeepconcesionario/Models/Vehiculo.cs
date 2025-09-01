using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace xeepconcesionario.Models
{
    public class Vehiculo
    {
        public int Id { get; set; }
        public string? Patente { get; set; }
        public string? Modelo { get; set; }
        public int? Año { get; set; }
        public string? Color { get; set; }

        [DataType(DataType.Date)]
        [Column(TypeName = "timestamp without time zone")]
        public DateTime FechaAlta { get; set; } = DateTime.Now;
        public string? Observacion { get; set; }

        public ICollection<ActividadVehiculo> Actividades { get; set; } = new List<ActividadVehiculo>();


    }
}
