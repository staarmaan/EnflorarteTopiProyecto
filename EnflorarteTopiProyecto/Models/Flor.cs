namespace EnflorarteTopiProyecto.Models
{
    public class Flor
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? FotoRuta { get; set; }
        public string? Descripcion { get; set; }
    }
}
