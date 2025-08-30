// Models/Dto/LookupDto.cs
namespace xeepconcesionario.Models.Dto
{
    public class LookupDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Tipo { get; set; } = ""; // "estado" | "condicion" | "tipobaja"
    }
}
