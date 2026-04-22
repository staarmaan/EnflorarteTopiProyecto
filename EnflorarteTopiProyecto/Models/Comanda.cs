/*
 -- Comandas
CREATE TABLE dbo.comanda (
    comanda_id INT IDENTITY(1,1) PRIMARY KEY,

    cliente_nombre NVARCHAR(100) NOT NULL,
    cliente_telefono NVARCHAR(20) NULL,
    direccion_entrega NVARCHAR(255) NULL,

    fecha_entrega DATE NOT NULL,
    hora_entrega TIME NULL,

    tipo_entrega NVARCHAR(10) NOT NULL, -- envio o recoger.
    nombre_arreglo NVARCHAR(150) NOT NULL,
    precio_arreglo DECIMAL(5,2) NOT NULL,

    foto_arreglo NVARCHAR(300) NULL,

    estado NVARCHAR(20) NOT NULL DEFAULT (N'pendiente'), -- pendiente, en_proceso, listo y entregado.
    liquidado BIT NOT NULL DEFAULT(0),

    anticipo_total DECIMAL(10,2) NOT NULL DEFAULT(0),
    anticipo_tipo NVARCHAR(50) NULL,

    pago_envio DECIMAL(10,2) NOT NULL DEFAULT(0),
    pago_arreglo DECIMAL(10,2) NOT NULL DEFAULT(0), -- El pago por envio y el pago del arreglo son separados. Aunque el pago del arreglo es el precio "original" por asi decirlo.


    repartidor_id INT NULL, -- usuario asignado, que tiene rol de repartidor.

    CONSTRAINT fk_comanda_repartidor FOREIGN KEY (repartidor_id)
        REFERENCES dbo.usuario(usuario_id)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION
);
GO
-- enums de la comanda.
ALTER TABLE dbo.comanda
    ADD CONSTRAINT chk_comanda_tipo_entrega CHECK (tipo_entrega IN (N'envio', N'recoger'));
GO

ALTER TABLE dbo.comanda
    ADD CONSTRAINT chk_comanda_estado CHECK (estado IN (N'pendiente', N'en_proceso', N'listo', N'entregado'));
GO

ALTER TABLE dbo.comanda
    ADD CONSTRAINT chk_comanda_anticipo_tipo CHECK (estado IN (N'porcentaje', N'minimo', N'manual'));
GO
 */

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnflorarteTopiProyecto.Models
{
    public enum TipoEntrega
    {
        envio,
        recoger,
        otro // Por si acaso ¯\_(ツ)_/¯.
    }

    public enum EstadoComanda
    {
        solicitado, // Las comandas tendran este estado si son creadas por cualquier usuario que no sea supervisor. Todos los usuarios que no sean supervisores podran crear comandas, pero es un super quien decide si se realizan o no.
        cancelado, // El supervisor puede cancelar una comanda (sea porque no fue aceptada la solicitud o porque algun imprevisto).
        pendiente, // Las comandas solicitadas por el supervisor tendran este estado inicialmente.
        en_proceso, // Las comandas en las que se estan trabajando tendran este estado.
        listo, // Las comandas que ya estan listas para ser entregadas o recogidas tendran este estado.
        entregado // Las comandas que ya fueron entregadas o recogidas tendran este estado. Osea que ya estan echas en su totalidad.
    }

    public enum AnticipoTipos
    {
        porcentaje, // Por ejemplo, si se cobro un 50% de anticipo en la temporada alta.
        minimo, // Por ejemplo, si se cobro un monto minimo de anticipo de $200 en temporada baja.
        manual // En caso de algun imprevisto que no se ajuste a los otros tipos.
    }


    public class Comanda
    {
        public int Id { get; set; }

        // Datos de usuarios.
        public int UsuarioId { get; set; } // Usuario que creo la comanda.
        [ForeignKey(nameof(UsuarioId))]
        public Usuario? UsuarioCreador { get; set; } // navegación al usuario que creó la comanda
       
        /*
        Creo que, en teoria, cualquier usuario podria ser repartidor. 
        Se le podria advertir al usuario asignando el repartiro de que esta eligiendo a uno que no lo es, pero si se le podria permitir.
        Por ejemplo, la supervisora podria asignarse a si misma como repartidora en una comanda aunque no tenga ese rol.
        */
        public int? RepartidorId { get; set; }
        [ForeignKey(nameof(RepartidorId))]
        public Usuario? RepartidorAsignado { get; set; } // navegación al usuario asignado como repartidor


        // Datos de estados.
        public EstadoComanda Estado { get; set; } = EstadoComanda.solicitado; // Por defecto, las comandas estan en solicitud.
        public bool Liquidado { get; set; } = false; // Por defecto, las comandas no estan liquidadas.

        // Datos del cliente y entrega.
        public string ClienteNombre { get; set; } = string.Empty;
        public string? ClienteTelefono { get; set; } // No se si deberia ser obligatorio u opcional.
        public string? LinkDireccion { get; set; } // link de la dirección del cliente.
        public string? DomicilioReferencias { get; set; } // Detalles visuales de la ubicacion del  cliente.
        public int? NumeroRuta { get; set; }

        public TipoEntrega TipoEntrega { get; set; }
        public string? DireccionEntrega { get; set; } // No seria obligatorio si el tipo de entrega es "recoger".
        public DateTime FechaEntrega { get; set; }
        public TimeSpan HoraEntrega { get; set; } // No estoy seguro de si deberia ser opcional u obligatorio.

        // Datos del arreglo.
        public string NombreArreglo { get; set; } = string.Empty;
        [Precision(7, 2)] // 7 digitos en total y dos decimales. Entonces, el valor maximo permitido seria 99,999.99
        [Range(typeof(decimal), "0", "99999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "El precio del arreglo debe ser entre 0 y 99,999.99.")]
        public decimal PrecioArreglo { get; set; }
        [Precision(7, 2)] // lo mismo que en NombreArreglo.
        [Range(typeof(decimal), "0", "99999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "El pago del envío debe ser entre 0 y 99,999.99.")]
        public decimal PagoEnvio { get; set; }

        [Range(1, 100, ErrorMessage = "La máxima cantiad de arreglos debe estar entre 1 y 100.")]
        public int CantidadArreglo { get; set; } = 1;

        public string? MensajeArreglo { get; set; }


        /*
        No se guarda la foto como tal en la bd, sino la ruta donde se encuentra almacenada. Si se guardara la foto en la bd, esta podria crecer mucho en tamaño y hacer las consultas muy lentas.
        Entonces, cuando se cargue una foto, se guarda una copia en alguna carpeta del sistema, y esta puede ser cargada despues.
        Que sea una copia es lo ideal, porque si se mueve o elimina la foto original, la foto de la comanda no se veria afectada.

        Como por ahora la app es solo local, la ruta puede ser una ruta relativa en el sistema de archivos local.
        Pero si se quisiera hacer una version web, habria que cambiar la logica para que las fotos se guarden en un servidor o servicio de almacenamiento en la nube, donde se puedan cargar las imagenes en cualquier dispositivo.
         */
        public string? FotoArregloRuta { get; set; }


        // Datos de anticipo.
        /*
        En la entrevista se menciono que, en temporada alta pide un anticipo del 50% del precio del arreglo. En la baja, pide un minimo de $200.
        Se podria guardar el porcentaje o el minimo en columnas respectivas de la tabala de comandas, segun el caso. 
        Sin embargo, seria un despericio de datos porque solo se usaria una de las dos columnas y usar una sola columna para guardar ambos casos podria ser confuso y algo sucio.
        En vez de eso, puedes conseguir el porcentaje de anticipo si el tipo fue de porcentaje, a partir del AnticipoTotal y el PrecioArreglo.
        Si se eligio el minimo u manual, entonces el AnticipoTotal ya seria el minimo :v
         */

        public AnticipoTipos? AnticipoTipo { get; set; } = AnticipoTipos.manual; // Por defecto, el tipo de anticipo es manual.
        [Precision(7, 2)] // x3             ʕ·͡ᴥ·ʔ
        [Range(typeof(decimal), "0", "99999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "El pago de anticipo debe ser entre 0 y 99,999.99.")]
        public decimal AnticipoPagoTotal { get; set; }
    }
}
