// Cuota.cs  (dejamos el nombre tal como lo usás en la vista: NumerocuotaId)
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using xeepconcesionario.Models;

public class Cuota
{
    public int CuotaId { get; set; }

    public int SolicitudId { get; set; }
    public Solicitud Solicitud { get; set; }

    public int Numerocuota { get; set; }  // lo mantenemos así para no tocar vistas ahora
    public int PlazoMeses { get; set; }
    public decimal MontoCuota { get; set; }
    public decimal SaldoCuota { get; set; }

    [DataType(DataType.Date)]
    [Column(TypeName = "timestamp")]
    public DateTime FechaVencimiento { get; set; }

    [DataType(DataType.Date)]
    [Column(TypeName = "timestamp")]
    public DateTime? FechaPago { get; set; }
    public enum Estado
    {
        Pendiente,
        Pagado
    }
    public Estado EstadoCuota { get; set; }

    public ICollection<Cobro> Cobros { get; set; } = new List<Cobro>();



}
