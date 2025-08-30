public class Localidad
{
    public int LocalidadId { get; set; }
    public int ProvinciaId { get; set; }
    public int RegionId { get; set; }
    public string NombreLocalidad { get; set; }
    public string CodigoPostal { get; set; }

    public Provincia Provincia { get; set; }
    public Region Region { get; set; }
    public ICollection<Cliente> Clientes { get; set; }
}