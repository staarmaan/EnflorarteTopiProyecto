namespace EnflorarteTopiProyecto.Models
{
    public class Arreglo
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;

        public string? FotoRuta { get; set; }
        /*
        OJO: No se guarda la foto como tal en la bd, sino la ruta donde se encuentra almacenada. Si se guardara la foto en la bd, esta podria crecer mucho en tamaño y hacer las consultas muy lentas.
        Entonces, cuando se cargue una foto, se guarda una copia en alguna carpeta del sistema, y esta puede ser cargada despues.
        Que sea una copia es lo ideal, porque si se mueve o elimina la foto original, la foto de la comanda no se veria afectada.

        Como por ahora la app es solo local, la dirrecion de la foto es una ruta relativa en el sistema de archivos local.
        Pero si se quisiera hacer una version web, habria que cambiar la logica para que las fotos se guarden en un servidor o servicio de almacenamiento en la nube, donde se puedan cargar las imagenes en cualquier dispositivo.
         */
        public string? Descripcion { get; set; }

        public ICollection<ArregloFlor> Flores { get; set; } = new List<ArregloFlor>();
    }
}
