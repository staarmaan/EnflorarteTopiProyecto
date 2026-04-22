using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EnflorarteTopiProyecto.Models
{
    public class FlorDto
    {
        [Display(Name = "nombre")]
        [Required(ErrorMessage = "El {0} es obligatorio.")]
        [StringLength(100, ErrorMessage = "El {0} no debe exceder {1} caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "foto")]
        public IFormFile? FotoArchivo { get; set; }

        [Display(Name = "descripción")]
        [StringLength(500, ErrorMessage = "La {0} no debe exceder {1} caracteres.")]
        public string? Descripcion { get; set; }
    }
}
