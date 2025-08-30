using xeepconcesionario.Models;

public class Contrato
{
    public int ContratoId { get; set; }
    public string NombreContrato { get; set; }
    public string? DescripcionContrato { get; set; }
    public int? PlazoMeses { get; set; }

    public ICollection<Solicitud> Solicitudes { get; set; }
}