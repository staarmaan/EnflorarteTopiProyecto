using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EnflorarteTopiProyecto.Service
{
    public class ApplicationDbContextPostgresFactory : IDesignTimeDbContextFactory<ApplicationDbContextPostgres>
    {
        public ApplicationDbContextPostgres CreateDbContext(string[] args)
        {
            var configuration = BuildConfiguration();
            var connectionString = configuration.GetConnectionString("PostgresConnection")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("PostgresConnection or DefaultConnection not found in configuration");

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContextPostgres>();
            optionsBuilder.UseNpgsql(connectionString);

            return new ApplicationDbContextPostgres(optionsBuilder.Options);
        }

        private static IConfigurationRoot BuildConfiguration()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
