namespace EnflorarteTopiProyecto.Models
{
    public class Arreglo
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? FotoRuta { get; set; }
        public string? Descripcion { get; set; }

        public ICollection<ArregloFlor> Flores { get; set; } = new List<ArregloFlor>();
    }
}
