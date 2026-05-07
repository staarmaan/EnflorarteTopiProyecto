namespace EnflorarteTopiProyecto.Models
{
    public class ArregloFlorSeleccionDto
    {
        public int FlorId { get; set; }
        public string FlorNombre { get; set; } = string.Empty;
        public bool Seleccionada { get; set; }
        public int Cantidad { get; set; } = 1;
        public string ColorSeleccionado { get; set; } = "a elegir";
        public List<string> ColoresDisponibles { get; set; } = new();
    }
}
