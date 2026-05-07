using System.ComponentModel.DataAnnotations;

namespace EnflorarteTopiProyecto.Models
{
    public class EditarInventarioFlorDto
    {
        public int FlorId { get; set; }
        public string NombreFlor { get; set; } = string.Empty;
        public string? DescripcionFlor { get; set; }
        public string? FotoRutaFlor { get; set; }

        [StringLength(50, ErrorMessage = "El color no debe exceder 50 caracteres.")]
        public string? NuevoColor { get; set; }

        public List<InventarioColorAjusteDto> Colores { get; set; } = new();
    }
}
