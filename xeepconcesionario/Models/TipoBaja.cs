using xeepconcesionario.Models;
public class TipoBaja
{
    public int TipoBajaId { get; set; }
    public string NombreTipoBaja { get; set; }

    public ICollection<Solicitud> Solicitudes { get; set; }
}