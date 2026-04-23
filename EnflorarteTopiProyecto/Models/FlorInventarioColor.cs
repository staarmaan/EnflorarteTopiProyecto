namespace EnflorarteTopiProyecto.Models
{
    public class FlorInventarioColor
    {
        public int Id { get; set; }
        public int FlorId { get; set; }
        public Flor Flor { get; set; } = null!;

        public string Color { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }
}
