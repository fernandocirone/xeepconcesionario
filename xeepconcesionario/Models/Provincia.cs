public class Provincia
{
    public int ProvinciaId { get; set; }
    public string NombreProvincia { get; set; }

    public ICollection<Localidad> Localidades { get; set; }
}