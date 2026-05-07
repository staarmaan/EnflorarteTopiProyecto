namespace EnflorarteTopiProyecto.Models
{
    public class Flor
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? FotoRuta { get; set; }
        public string? Descripcion { get; set; }

        public ICollection<ArregloFlor> Arreglos { get; set; } = new List<ArregloFlor>();
        public ICollection<FlorInventarioColor> InventarioColores { get; set; } = new List<FlorInventarioColor>();
    }
}
