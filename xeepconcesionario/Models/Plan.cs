using xeepconcesionario.Models;

namespace xeepconcesionario
{
    public class Plan
    {
        public int PlanId { get; set; }

        public string Codigo { get; set; } // Ej: "C60"
        public string Modelo { get; set; } // Ej: "VW Polo MSI / Fiat Cronos"

        public TipoPlan? TipoPlan { get; set; }

        public int CuotaApertura { get; set; }        // 5 o 10
        public decimal AdelantoMensual { get; set; }  // Ej: 250000
        public decimal Sellado { get; set; }          // Ej: 100000
        public decimal CuotaIngreso { get; set; }     // Ej: 350000

        // Si querés relacionarlo con solicitudes:
        public ICollection<Solicitud> Solicitudes { get; set; }
    }
}
