using System.ComponentModel.DataAnnotations.Schema;

namespace EnflorarteTopiProyecto.Models
{
    public class ComandaFlor
    {
        public int ComandaId { get; set; }
        [ForeignKey(nameof(ComandaId))]
        public Comanda Comanda { get; set; } = null!;

        public int FlorId { get; set; }
        [ForeignKey(nameof(FlorId))]
        public Flor Flor { get; set; } = null!;

        public int Cantidad { get; set; }
        public string ColorSeleccionado { get; set; } = "a elegir";
    }
}
