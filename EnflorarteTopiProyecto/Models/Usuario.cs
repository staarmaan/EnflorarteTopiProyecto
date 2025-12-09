using Microsoft.EntityFrameworkCore;

namespace EnflorarteTopiProyecto.Models
{
    public enum RolUsuario
    {
        supervisor,
        ventas,
        florista,
        repartidor
    }
    public class Usuario
    {
        public int Id { get; set; } = 0;
        public string Nombre { get; set; } = string.Empty;
        public RolUsuario Rol { get; set; } = RolUsuario.supervisor;
        public string Contrasena { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
    }
}
