using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace xeepconcesionario.Models
{
    public class Contrato
    {
        [Key]
        public int ContratoId { get; set; }

        // --- Relación con Solicitud (1 a 1) ---
        [Required]
        public int SolicitudId { get; set; }
        public Solicitud Solicitud { get; set; }

        // --- Relación con Vehículo ---
        [Required]
        public int VehiculoId { get; set; }
        public Vehiculo Vehiculo { get; set; }

        [Required]
        [StringLength(100)]
        public string NombreContrato { get; set; } = string.Empty;

        public string? DescripcionContrato { get; set; }

        /// <summary>
        /// Plazo en meses del nuevo contrato
        /// </summary>
        public int? PlazoMeses { get; set; }

        /// <summary>
        /// Cantidad de cuotas redefinidas en el contrato
        /// </summary>
        public int CantidadCuotas { get; set; }

        /// <summary>
        /// Valor de cada cuota redefinida
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoCuota { get; set; }

        /// <summary>
        /// Total pagado acumulado en las cuotas anteriores,
        /// convertido a un único movimiento inicial
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoPagadoAcumulado { get; set; }

        /// <summary>
        /// Propiedad adicional solicitada: valor de transferencia
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorTransferencia { get; set; }
    }
}
