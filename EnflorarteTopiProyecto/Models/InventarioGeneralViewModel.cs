namespace EnflorarteTopiProyecto.Models
{
    public class InventarioGeneralViewModel
    {
        public List<string> Colores { get; set; } = new();
        public List<InventarioGeneralFilaViewModel> Filas { get; set; } = new();
    }

    public class InventarioGeneralFilaViewModel
    {
        public int FlorId { get; set; }
        public string FlorNombre { get; set; } = string.Empty;
        public Dictionary<string, int> CantidadesPorColor { get; set; } = new();
    }
}
