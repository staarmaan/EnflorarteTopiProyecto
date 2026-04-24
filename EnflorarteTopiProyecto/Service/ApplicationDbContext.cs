using EnflorarteTopiProyecto.Models;
using Microsoft.EntityFrameworkCore;

namespace EnflorarteTopiProyecto.Service
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Comanda> Comandas { get; set; } = null!;
        public DbSet<Flor> Flores { get; set; } = null!;
        public DbSet<FlorInventarioColor> FloresInventarioColores { get; set; } = null!;
        public DbSet<Arreglo> Arreglos { get; set; } = null!;
        public DbSet<ArregloFlor> ArreglosFlores { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder) // Codigo para que se convierta el Enum de roles a string, o algo asi.
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("usuario"); // asegura el nombre de tabla

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                      .HasColumnName("usuario_id");

                entity.Property(e => e.Nombre)
                      .HasColumnName("nombre")
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.Contrasena)
                      .HasColumnName("contrasena")
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.Activo)
                      .HasColumnName("activo");

                // Mapea el enum a string (coincidiendo con el CHECK de la BD)
                entity.Property(e => e.Rol)
                      .HasColumnName("rol")
                      .HasConversion<string>() // almacena como nvarchar
                      .IsRequired()
                      .HasMaxLength(20)
                      .IsUnicode(true);
            });

            modelBuilder.Entity<Comanda>(entity =>
            {
                        entity.ToTable("comanda", t =>
                        {
                              // Constraints para evitar valores inválidos (protección en BD)
                              t.HasCheckConstraint("CK_Comanda_PrecioArreglo_NonNegative", "precio_arreglo >= 0");
                              t.HasCheckConstraint("CK_Comanda_PagoEnvio_NonNegative", "pago_envio >= 0");
                              t.HasCheckConstraint("CK_Comanda_AnticipoTotal_NonNegative", "anticipo_total >= 0");
                              t.HasCheckConstraint("CK_Comanda_CantidadArreglo_ValidRange", "cantidad_arreglo BETWEEN 1 AND 100");
                              t.HasCheckConstraint("CK_Comanda_NumeroRuta_PositiveOrNull", "numero_ruta IS NULL OR numero_ruta > 0");
                        });

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                      .HasColumnName("comanda_id");

                // Relaciones con Usuario
                entity.Property(e => e.UsuarioId)
                      .HasColumnName("usuario_id")
                      .IsRequired();

                entity.Property(e => e.RepartidorId)
                      .HasColumnName("repartidor_id");

                entity.HasOne(e => e.UsuarioCreador)
                      .WithMany() // Indica que un usuario puede tener muchas comandas
                      .HasForeignKey(e => e.UsuarioId)
                      .OnDelete(DeleteBehavior.NoAction); // Si se borra un usuario, no borrar las comandas asociadas.

                entity.HasOne(e => e.RepartidorAsignado)
                      .WithMany() // x2
                      .HasForeignKey(e => e.RepartidorId)
                      .OnDelete(DeleteBehavior.NoAction); // x2

                // Datos del cliente y entrega
                entity.Property(e => e.ClienteNombre)
                      .HasColumnName("cliente_nombre")
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.ClienteTelefono)
                      .HasColumnName("cliente_telefono")
                      .HasMaxLength(20);

                entity.Property(e => e.LinkDireccion)
                      .HasColumnName("link_direccion")
                      .HasMaxLength(1000);

                entity.Property(e => e.DomicilioReferencias)
                      .HasColumnName("domicilio_referencias")
                      .HasMaxLength(500);

                entity.Property(e => e.NumeroRuta)
                      .HasColumnName("numero_ruta");

                entity.Property(e => e.DireccionEntrega)
                      .HasColumnName("direccion_entrega")
                      .HasMaxLength(255);

                entity.Property(e => e.FechaEntrega)
                      .HasColumnName("fecha_entrega")
                      .HasColumnType("date");

                entity.Property(e => e.HoraEntrega)
                      .HasColumnName("hora_entrega")
                      .HasColumnType("time");

                entity.Property(e => e.TipoEntrega)
                      .HasColumnName("tipo_entrega")
                      .HasConversion<string>()
                      .HasMaxLength(10)
                      .IsUnicode(true);

                // Datos del arreglo
                entity.Property(e => e.ArregloId)
                      .HasColumnName("arreglo_id");

                entity.HasOne(e => e.Arreglo)
                      .WithMany()
                      .HasForeignKey(e => e.ArregloId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.PrecioArreglo)
                      .HasColumnName("precio_arreglo")
                      .HasPrecision(7, 2);

                entity.Property(e => e.PagoEnvio)
                      .HasColumnName("pago_envio")
                      .HasPrecision(7, 2);

                entity.Property(e => e.CantidadArreglo)
                      .HasColumnName("cantidad_arreglo")
                      .HasDefaultValue(1);

                entity.Property(e => e.MensajeArreglo)
                      .HasColumnName("mensaje_arreglo")
                      .HasMaxLength(500);

                // Estados y anticipos
                entity.Property(e => e.Estado)
                      .HasColumnName("estado")
                      .HasConversion<string>()
                      .HasMaxLength(20)
                      .IsUnicode(true);

                entity.Property(e => e.Liquidado)
                      .HasColumnName("liquidado");

                entity.Property(e => e.AnticipoTipo)
                      .HasColumnName("anticipo_tipo")
                      .HasConversion<string>()
                      .HasMaxLength(50)
                      .IsUnicode(true);

                entity.Property(e => e.AnticipoPagoTotal)
                      .HasColumnName("anticipo_total")
                      .HasPrecision(7, 2);

            });

            modelBuilder.Entity<Flor>(entity =>
            {
                entity.ToTable("flor");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                      .HasColumnName("flor_id");

                entity.Property(e => e.Nombre)
                      .HasColumnName("nombre")
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.FotoRuta)
                      .HasColumnName("foto_ruta")
                      .HasMaxLength(300);

                entity.Property(e => e.Descripcion)
                      .HasColumnName("descripcion")
                      .HasMaxLength(500);
            });

            modelBuilder.Entity<Arreglo>(entity =>
            {
                entity.ToTable("arreglo");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                      .HasColumnName("arreglo_id");

                entity.Property(e => e.Nombre)
                      .HasColumnName("nombre")
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.FotoRuta)
                      .HasColumnName("foto_ruta")
                      .HasMaxLength(300);

                entity.Property(e => e.Descripcion)
                      .HasColumnName("descripcion")
                      .HasMaxLength(500);
            });

            modelBuilder.Entity<ArregloFlor>(entity =>
            {
                entity.ToTable("arreglo_flor");

                entity.HasKey(e => new { e.ArregloId, e.FlorId });

                entity.Property(e => e.ArregloId)
                      .HasColumnName("arreglo_id");

                entity.Property(e => e.FlorId)
                      .HasColumnName("flor_id");

                entity.Property(e => e.Cantidad)
                      .HasColumnName("cantidad");

                entity.Property(e => e.ColorSeleccionado)
                      .HasColumnName("color_seleccionado")
                      .IsRequired()
                      .HasMaxLength(50);

                entity.HasOne(e => e.Arreglo)
                      .WithMany(e => e.Flores)
                      .HasForeignKey(e => e.ArregloId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Flor)
                      .WithMany(e => e.Arreglos)
                      .HasForeignKey(e => e.FlorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<FlorInventarioColor>(entity =>
            {
                entity.ToTable("flor_inventario_color");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                      .HasColumnName("flor_inventario_color_id");

                entity.Property(e => e.FlorId)
                      .HasColumnName("flor_id");

                entity.Property(e => e.Color)
                      .HasColumnName("color")
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.Cantidad)
                      .HasColumnName("cantidad");

                entity.HasIndex(e => new { e.FlorId, e.Color })
                      .IsUnique();

                entity.HasOne(e => e.Flor)
                      .WithMany(e => e.InventarioColores)
                      .HasForeignKey(e => e.FlorId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        protected ApplicationDbContext()
        {
        }
    }
}
