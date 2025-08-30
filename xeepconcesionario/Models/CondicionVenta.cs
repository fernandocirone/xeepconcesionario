using xeepconcesionario.Models;

public class CondicionVenta
{
    public int CondicionVentaId { get; set; }
    public string NombreCondicionVenta { get; set; }

    public ICollection<Solicitud> Solicitudes { get; set; }
}