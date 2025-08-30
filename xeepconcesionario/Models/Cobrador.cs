public class Cobrador
{
    public int CobradorId { get; set; }
    public string NombreCobrador { get; set; }

    public ICollection<Cobro> Cobros { get; set; }
}