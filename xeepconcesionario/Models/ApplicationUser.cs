// Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;

namespace xeepconcesionario.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? NombreCompleto { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }

        public ICollection<ApplicationUserTipoUsuario> TiposUsuario { get; set; } = new List<ApplicationUserTipoUsuario>();
    }
}
