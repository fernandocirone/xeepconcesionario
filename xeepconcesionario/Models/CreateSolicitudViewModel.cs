using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace xeepconcesionario.Models
{
    public class CreateSolicitudViewModel
    {
        public int SolicitudId { get; set; }

        // --- Cliente ---
        public int? ClienteId { get; set; }
        public bool CrearClienteNuevo { get; set; } = true;
        public Cliente ClienteNuevo { get; set; } = new();

        // --- Usuarios ---
        public string? VendedorUserId { get; set; }
        public string? SupervisorUserId { get; set; }
        public string? JefeVentasUserId { get; set; }

        // --- Auditoría / quién carga ---
        public string? UsuarioId { get; set; }

        // --- Solicitud ---
        public int? ContratoId { get; set; }
        public int PlanId { get; set; }
        public int CondicionVentaId { get; set; }
        public int TipoBajaId { get; set; }
        public int EstadoId { get; set; } = 1;

        public int NumeroSolicitud { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FechaCarga { get; set; }


        [DataType(DataType.Date)]
        public DateTime? FechaSuscripcion { get; set; }

        public decimal ValorSellado1 { get; set; }
        public decimal ValorSellado2 { get; set; }

        public string? ObservacionSolicitud { get; set; }

        // --- Parámetros de cuotas ---
        public int CantidadCuotas { get; set; } = 99;
        public DateTime? FechaPrimerVencimiento { get; set; }
        public decimal? ImporteCuota { get; set; }

        public List<ActividadSolicitud> Actividades { get; set; } = new();

        // --- Campos “de nombre” para usar en Details ---
        public string? PlanNombre { get; set; }
        public string? CondicionVentaNombre { get; set; }
        public string? TipoBajaNombre { get; set; }
        public string? EstadoNombre { get; set; }
        public string? VendedorNombre { get; set; }
        public string? SupervisorNombre { get; set; }
        public string? JefeVentasNombre { get; set; }
        public string? ClienteNombre { get; set; }
        public string? LocalidadNombre { get; set; }
    }
}
