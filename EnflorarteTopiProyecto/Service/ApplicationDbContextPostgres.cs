using Microsoft.EntityFrameworkCore;

namespace EnflorarteTopiProyecto.Service
{
    public class ApplicationDbContextPostgres : ApplicationDbContext
    {
        public ApplicationDbContextPostgres(DbContextOptions<ApplicationDbContextPostgres> options)
            : base(options)
        {
        }
    }
}
