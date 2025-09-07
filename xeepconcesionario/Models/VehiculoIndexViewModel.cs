namespace xeepconcesionario.Models
{
    public class VehiculoIndexViewModel
    {
        public int Id { get; set; }
        public string? Patente { get; set; }
        public TipoVehiculo Tipo { get; set; }
        public string? Modelo { get; set; }
        public int? Año { get; set; }
        public string? Color { get; set; }
        public decimal? PrecioCompra { get; set; }
        public decimal? Valor { get; set; }
        public DateTime? FechaAlta { get; set; }
        public string? Observacion { get; set; }

        // Datos de la última actividad
        public decimal MontoActividad { get; set; }
        public string? Sucursal { get; set; }
        public string? UltimaActividadNombre { get; set; }
        public DateTime? UltimaActividadFecha { get; set; }
    }
}
