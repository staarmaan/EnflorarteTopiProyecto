namespace EnflorarteTopiProyecto.Models
{
    public class ArregloFlor
    {
        public int ArregloId { get; set; }
        public Arreglo Arreglo { get; set; } = null!;

        public int FlorId { get; set; }
        public Flor Flor { get; set; } = null!;

        public int Cantidad { get; set; }
        public string ColorSeleccionado { get; set; } = "a elegir";
    }
}
