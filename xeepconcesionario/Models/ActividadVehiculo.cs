using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace xeepconcesionario.Models
{
    public class ActividadVehiculo
    {
        public int Id { get; set; }
        public int? VehiculoId { get; set; }

        public Vehiculo? Vehiculo { get; set; }

        public int? TipoActividadVehiculoId { get; set; }
        public TipoActividadVehiculo? TipoActividadVehiculo { get; set; }

        public decimal? Monto { get; set; }

        public int? SucursalId { get; set; }
        public Sucursal? Sucursal { get; set; }

        [DataType(DataType.Date)]
        [Column(TypeName = "timestamp without time zone")]
        public DateTime Fecha { get; set; }

        // Texto largo de observación
        [DataType(DataType.MultilineText)]
        public string? Observacion { get; set; }

        // Usuario que registró la actividad
        [Required]
        public string UsuarioId { get; set; } = null!;
        public ApplicationUser Usuario { get; set; } = null!;

    }
}
