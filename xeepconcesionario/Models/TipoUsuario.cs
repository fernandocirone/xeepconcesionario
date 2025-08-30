// Models/TipoUsuario.cs
namespace xeepconcesionario.Models
{
    public class TipoUsuario
    {
        public int TipousuarioId { get; set; }         // 1=Vendedor, 2=Supervisor, 3=JefeVentas
        public string Nombretipousuario { get; set; } = null!;

        public ICollection<ApplicationUserTipoUsuario> Usuarios { get; set; } = new List<ApplicationUserTipoUsuario>();
    }
}
