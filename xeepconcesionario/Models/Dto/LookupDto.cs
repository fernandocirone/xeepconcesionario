// Models/Dto/LookupDto.cs
using System.ComponentModel.DataAnnotations;

namespace xeepconcesionario.Models.Dto
{
    public class LookupDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Tipo { get; set; } = ""; // "estado" | "condicion" | "tipobaja"
                                               // Nuevo: sólo aplica a "estado"
                                               // Validación HEX #RRGGBB o #RGB
        public string? Direccion { get; set; }

        [RegularExpression("^#(?:[0-9a-fA-F]{3}){1,2}$", ErrorMessage = "Color inválido (use #RRGGBB).")]
        public string? Color { get; set; }
    }
}
