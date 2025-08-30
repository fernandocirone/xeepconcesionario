public class Region
{
    public int RegionId { get; set; }
    public string NombreRegion { get; set; }

    public ICollection<Localidad> Localidades { get; set; }
}