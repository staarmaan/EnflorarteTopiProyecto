using System.ComponentModel.DataAnnotations;

namespace EnflorarteTopiProyecto.Models
{
    public class ComandaDto
    {
        /*
        Funciona igual que UsuarioDto.cs pero para las comandasd :p.
        */
        
        public int Id { get; set; }

        // Usuarios
        [Required]
        public int UsuarioId { get; set; }
        public Usuario? UsuarioCreador { get; set; }

        public int? RepartidorId { get; set; }
        public Usuario? RepartidorAsignado { get; set; }

        // Estado y liquidación
        [EnumDataType(typeof(EstadoComanda))]
        public EstadoComanda Estado { get; set; }
        public bool Liquidado { get; set; }

        // Datos del cliente y entrega
        [Required]
        [StringLength(100)]
        public string ClienteNombre { get; set; } = string.Empty;

        [StringLength(20)]
        [Phone]
        public string? ClienteTelefono { get; set; }

        [Required]
        [EnumDataType(typeof(TipoEntrega))]
        public TipoEntrega TipoEntrega { get; set; }

        [StringLength(255)]
        public string? DireccionEntrega { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime FechaEntrega { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan HoraEntrega { get; set; }


        // Datos del arreglo
        [Required]
        [StringLength(150)]
        public string NombreArreglo { get; set; } = string.Empty;

        [Required]
        [Range(typeof(decimal), "0", "99999.99", ErrorMessage = "El precio del arreglo debe ser entre 0 y 99,999.99.")]
        public decimal PrecioArreglo { get; set; }

        [Range(typeof(decimal), "0", "99999.99", ErrorMessage = "El pago del envío debe ser entre 0 y 99,999.99.")]
        public decimal PagoEnvio { get; set; }

        [StringLength(300)]
        public string? FotoArregloRuta { get; set; }

        // Anticipo
        [EnumDataType(typeof(AnticipoTipos))]
        public AnticipoTipos? AnticipoTipo { get; set; }

        [Range(typeof(decimal), "0", "99999.99", ErrorMessage = "El pago de anticipo debe ser entre 0 y 99,999.99.")]
        public decimal AnticipoPagoTotal { get; set; }
    }
}
