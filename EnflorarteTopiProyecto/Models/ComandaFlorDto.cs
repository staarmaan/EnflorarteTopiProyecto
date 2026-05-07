using System;

namespace EnflorarteTopiProyecto.Models
{
    public class ComandaFlorDto
    {
        public int FlorId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public string? ColorSeleccionado { get; set; }
    }
}
