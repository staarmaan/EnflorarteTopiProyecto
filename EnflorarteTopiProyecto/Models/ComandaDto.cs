using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EnflorarteTopiProyecto.Models
{
    public class ComandaDto
    {
        /*
        Funciona igual que UsuarioDto.cs pero para las comandasd :p.
        */
        
        public int Id { get; set; }

        // Usuarios
        [Display(Name = "usuario creador")]
        [Required(ErrorMessage = "Es obligatorio asignar un {0} a la comanda.")]
        public int UsuarioId { get; set; }
        public Usuario? UsuarioCreador { get; set; }

        [Display(Name = "usuario repartidor")]
        public int? RepartidorId { get; set; }
        public Usuario? RepartidorAsignado { get; set; }

        // Estado y liquidación
        [Display(Name = "estado de la comanda")]
        [EnumDataType(typeof(EstadoComanda), ErrorMessage = "El {0} seleccionado no es válido.")]
        public EstadoComanda Estado { get; set; }
        public bool Liquidado { get; set; }

        // Datos del cliente y entrega
        [Display(Name = "nombre del cliente")]
        [Required(ErrorMessage = "El {0} es obligatorio.")]
        [StringLength(100, ErrorMessage = "El {0} no debe exceder {1} carácteres.")]
        public string ClienteNombre { get; set; } = string.Empty;

        [Display(Name = "teléfono del cliente")]
        [Required(ErrorMessage = "El {0} es obligatorio.")]
        [StringLength(20, ErrorMessage = "El {0} no debe exceder {1} carácteres.")]
        [Phone(ErrorMessage = "El {0} no tiene un formato válido.")]
        public string? ClienteTelefono { get; set; }

        [Display(Name = "link de dirección")]
        [StringLength(1000, ErrorMessage = "El {0} no debe exceder {1} carácteres.")]
        [Url(ErrorMessage = "El {0} no tiene un formato válido.")]
        public string? LinkDireccion { get; set; }

        [Display(Name = "referencias de domicilio")]
        [StringLength(500, ErrorMessage = "Las {0} no deben exceder {1} carácteres.")]
        public string? DomicilioReferencias { get; set; }

        [Display(Name = "número de ruta")]
        [Range(1, int.MaxValue, ErrorMessage = "El {0} debe ser mayor a 0.")]
        public int? NumeroRuta { get; set; }

        [Display(Name = "tipo de entrega")]
        [Required(ErrorMessage = "El {0} es obligatorio.")]
        [EnumDataType(typeof(TipoEntrega), ErrorMessage = "El {0} seleccionado no es válido.")]
        public TipoEntrega TipoEntrega { get; set; }

        [Display(Name = "dirección de entrega")]
        [StringLength(255, ErrorMessage = "La {0} no debe exceder {1} carácteres.")]
        public string? DireccionEntrega { get; set; }

        [Display(Name = "fecha de entrega")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaEntrega { get; set; }

        [Display(Name = "hora de entrega")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [DataType(DataType.Time)]
        public TimeSpan HoraEntrega { get; set; }


        // Datos del arreglo
        [Display(Name = "nombre del arreglo")]
        [Required(ErrorMessage = "El {0} es obligatorio.")]
        [StringLength(150, ErrorMessage = "El {0} no debe exceder {1} carácteres.")]
        public string NombreArreglo { get; set; } = string.Empty;

        [Display(Name = "precio del arreglo")]
        [Required(ErrorMessage = "El {0} es obligatorio.")]
        [Range(typeof(decimal), "0", "99999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "El {0} debe ser entre 0 y 99,999.99.")]
        public decimal PrecioArreglo { get; set; }

        [Display(Name = "pago del envío")]
        [Range(typeof(decimal), "0", "99999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "El {0} debe ser entre 0 y 99,999.99.")]
        public decimal PagoEnvio { get; set; }

        [Display(Name = "cantidad de arreglos")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [Range(1, 100, ErrorMessage = "La {0} debe estar entre {1} y {2}.")]
        public int CantidadArreglo { get; set; } = 1;

        [Display(Name = "mensaje del arreglo")]
        [StringLength(500, ErrorMessage = "El {0} no debe exceder {1} carácteres.")]
        public string? MensajeArreglo { get; set; }

        [Display(Name = "ruta de foto del arreglo")]
        [StringLength(300, ErrorMessage = "La {0} no debe exceder {1} carácteres.")]
        public string? FotoArregloRuta { get; set; }

        // Archivo de imagen subido desde el formulario
        [Display(Name = "foto del arreglo (archivo)")]
        public IFormFile? FotoArregloArchivo { get; set; }

        // true para eliminar la foto existente
        [Display(Name = "eliminar foto existente")]
        public bool? EliminarFoto { get; set; } = false;

        // Anticipo
        [Display(Name = "tipo de anticipo")]
        [EnumDataType(typeof(AnticipoTipos), ErrorMessage = "El {0} seleccionado no es válido.")]
        public AnticipoTipos? AnticipoTipo { get; set; }

        [Display(Name = "pago total de anticipo")]
        [Range(typeof(decimal), "0", "99999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "El {0} debe ser entre 0 y 99,999.99.")]
        public decimal AnticipoPagoTotal { get; set; }
    }
}
