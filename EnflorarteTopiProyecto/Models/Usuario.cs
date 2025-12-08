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
        public int Id { get; set; }
        public string Nombre { get; set; }
        public RolUsuario Rol { get; set; }
        public string Contrasena { get; set; }
        public bool Activo { get; set; }
    }
}
