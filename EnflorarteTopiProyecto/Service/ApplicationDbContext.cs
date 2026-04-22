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
                entity.ToTable("comanda");

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
                entity.Property(e => e.NombreArreglo)
                      .HasColumnName("nombre_arreglo")
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(e => e.PrecioArreglo)
                      .HasColumnName("precio_arreglo")
                      .HasPrecision(7, 2);

                entity.Property(e => e.PagoEnvio)
                      .HasColumnName("pago_envio")
                      .HasPrecision(7, 2);

                entity.Property(e => e.FotoArregloRuta)
                      .HasColumnName("foto_arreglo_ruta")
                      .HasMaxLength(300);

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

                // Constraints para evitar valores negativos (protección en BD)
                entity.HasCheckConstraint("CK_Comanda_PrecioArreglo_NonNegative", "[precio_arreglo] >= 0");
                entity.HasCheckConstraint("CK_Comanda_PagoEnvio_NonNegative", "[pago_envio] >= 0");
                entity.HasCheckConstraint("CK_Comanda_AnticipoTotal_NonNegative", "[anticipo_total] >= 0");
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
        }

        protected ApplicationDbContext()
        {
        }
    }
}
