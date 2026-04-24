using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;

namespace OpcionesBd
{
    public static class OpcionesBD
    {
        /// <summary>
        /// Configures the database options based on the specified type.
        /// </summary>
        /// <param name="tipo">Debe ser "sql" o "postgres". Si no es ninguna de estas, usara sql por defecto.</param>
        /// <param name="options">DbContextOptionsBuilder que viene de AddDbContext y es necesario para conectarse a la bd.</param>
        /// <param name="builder">El que viene de WebApplication en Program.cs.</param>
    public static void UsarBD(DbContextOptionsBuilder options, string tipo, WebApplicationBuilder builder)
    {
        string connectionString;
        var tipoNormalizado = (tipo ?? "sql").Trim().ToLowerInvariant();

        switch (tipoNormalizado)
        {
            case "postgres":
                connectionString = builder.Configuration.GetConnectionString("PostgresConnection")
                    ?? builder.Configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("PostgresConnection or DefaultConnection not found in configuration");
                options.UseNpgsql(connectionString);
                break;
            default:
                connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("DefaultConnection not found in configuration");
                options.UseSqlServer(connectionString);
                break;
        }
    }
    }
}