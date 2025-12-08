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

        protected override void OnModelCreating(ModelBuilder modelBuilder) // Codigo para que se convierta el Enum de roles a string, o algo asi.
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("usuario"); // asegura el nombre de tabla

                entity.HasKey(e => e.Id);

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
        }

        protected ApplicationDbContext()
        {
        }
    }
}
