using System;
using System.Collections.Generic;

namespace xeepconcesionario.Models
{
    public class SolicitudDetailsViewModel
    {
        public int SolicitudId { get; set; }
        public int NumeroSolicitud { get; set; }

        // Usuarios
        public string? VendedorNombre { get; set; }
        public string? SupervisorNombre { get; set; }
        public string? JefeVentasNombre { get; set; }

        // Cliente
        public int ClienteId { get; set; }   // 👈 Agregalo
        public string? ClienteNombre { get; set; }
        public Cliente ClienteNuevo { get; set; } = new();
        public string? LocalidadNombre { get; set; }

        // Solicitud
        public string? PlanNombre { get; set; }
        public string? CondicionVentaNombre { get; set; }
        public string? TipoBajaNombre { get; set; }
        public string? EstadoNombre { get; set; }

        public DateTime? FechaCarga { get; set; }
        public DateTime? FechaSuscripcion { get; set; }
        public decimal ValorSellado1 { get; set; }
        public decimal ValorSellado2 { get; set; }
        public int CantidadCuotas { get; set; }
        public DateTime? FechaPrimerVencimiento { get; set; }
        public decimal? ImporteCuota { get; set; }
        public string? ObservacionSolicitud { get; set; }

        // Contrato asociado
        public Contrato? Contrato { get; set; }

        // Historial de actividades
        public List<ActividadSolicitud> Actividades { get; set; } = new();
    }
}
