using xeepconcesionario.Models;
public class Estado
{
    public int EstadoId { get; set; }
    public string NombreEstado { get; set; }
    public string? Color { get; set; }

    public ICollection<Solicitud> Solicitudes { get; set; }
}