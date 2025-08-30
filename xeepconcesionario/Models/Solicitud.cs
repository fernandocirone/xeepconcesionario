// Models/Solicitud.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace xeepconcesionario.Models
{
    public class Solicitud
    {
        public int SolicitudId { get; set; }
        public int? ContratoId { get; set; }

        public  int NumeroSolicitud { get; set; }

        // ?? FKs a ApplicationUser (AspNetUsers) — son string
        public string? VendedorUserId { get; set; }
        public string? SupervisorUserId { get; set; }
        public string? JefeVentasUserId { get; set; }

        public string UsuarioId { get; set; } = null!;   // quien carga la solicitud (si aplica)

        public int ClienteId { get; set; }
        public int PlanId { get; set; }
        public int CondicionVentaId { get; set; }
        public int TipoBajaId { get; set; }
        public int EstadoId { get; set; }

        [DataType(DataType.Date)]
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? FechaCarga { get; set; }

        [DataType(DataType.Date)]
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? FechaSuscripcion { get; set; }

        public decimal ValorSellado1 { get; set; }
        public decimal ValorSellado2 { get; set; }

        public string? ObservacionSolicitud { get; set; }


        // Navigations
        public ApplicationUser? Vendedor { get; set; }
        public ApplicationUser? Supervisor { get; set; }
        public ApplicationUser? JefeVentas { get; set; }
        public ApplicationUser Usuario { get; set; } = null!;
        public Cliente Cliente { get; set; } = null!;
        public Plan Plan { get; set; } = null!;
        public CondicionVenta CondicionVenta { get; set; } = null!;
        public TipoBaja TipoBaja { get; set; } = null!;
        public Estado Estado { get; set; } = null!;

        public ICollection<Cuota> Cuotas { get; set; } = new List<Cuota>();
        public ICollection<Cobro> Cobros { get; set; } = new List<Cobro>();

        public ICollection<ActividadSolicitud> Actividades { get; set; } = new List<ActividadSolicitud>();

    }
}
