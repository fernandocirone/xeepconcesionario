using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace xeepconcesionario.Models
{
    public class Vehiculo
    {
        public int Id { get; set; }
        public string? Patente { get; set; }

        [Required]
        public TipoVehiculo Tipo { get; set; }

        public string? Modelo { get; set; }
        public int? Año { get; set; }
        public string? Color { get; set; }
        public decimal? PrecioCompra { get; set; }

        public decimal? Valor { get; set; }

        [DataType(DataType.Date)]
        [Column(TypeName = "timestamp without time zone")]
        public DateTime FechaAlta { get; set; } = DateTime.Now;
        public string? Observacion { get; set; }

        public EstadoVehiculo EstadoVehiculo { get; set; }

        public ICollection<ActividadVehiculo> Actividades { get; set; } = new List<ActividadVehiculo>();
        public ICollection<Contrato> Contratos { get; set; } = new List<Contrato>();



    }
}
