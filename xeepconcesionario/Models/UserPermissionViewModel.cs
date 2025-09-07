namespace xeepconcesionario.Models
{
    public class UserPermissionViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public List<PermissionGroup> Grupos { get; set; } = new();
    }

    public class PermissionGroup
    {
        public string Nombre { get; set; }
        public List<PermissionItem> Permisos { get; set; } = new();
    }

    public class PermissionItem
    {
        public string Nombre { get; set; }
        public bool Asignado { get; set; }
    }


}
