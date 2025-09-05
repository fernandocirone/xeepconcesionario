// Cobro.cs
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using xeepconcesionario.Models;

public class Cobro
{
    public int CobroId { get; set; }

    public int SolicitudId { get; set; }
    public Solicitud Solicitud { get; set; }

    public string UsuarioId { get; set; }  
    
    public ApplicationUser? Usuario { get; set; }

    public int CuotaId { get; set; }        // un cobro -> una cuota
    public Cuota Cuota { get; set; }

    [DataType(DataType.Date)]
    [Column(TypeName = "timestamp")]
    public DateTime Fecha { get; set; }
    public decimal Monto { get; set; }

    public string? ObservacionCobro { get; set; }
}
