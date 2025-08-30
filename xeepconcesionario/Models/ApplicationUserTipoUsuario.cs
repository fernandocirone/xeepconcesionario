namespace xeepconcesionario.Models
{
    public class ApplicationUserTipoUsuario
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int TipoUsuarioId { get; set; }
        public TipoUsuario TipoUsuario { get; set; }
    }
}
