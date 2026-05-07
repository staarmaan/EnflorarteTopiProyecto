using System.ComponentModel.DataAnnotations;

namespace EnflorarteTopiProyecto.Models
{
    public class InventarioColorAjusteDto
    {
        public int InventarioId { get; set; }
        public string Color { get; set; } = string.Empty;
        public int CantidadActual { get; set; }

        [Range(1, 999, ErrorMessage = "Ingresa una cantidad válida para agregar.")]
        public int? AgregarCantidad { get; set; }

        [Range(1, 999, ErrorMessage = "Ingresa una cantidad válida para quitar.")]
        public int? QuitarCantidad { get; set; }
    }
}
