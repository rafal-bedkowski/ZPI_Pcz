using Microsoft.EntityFrameworkCore;

namespace WebStore.DAL.EF;

// Osobny kontekst dla migracji SQL Server (patrz SqliteDbContext).
public class SqlServerDbContext : ApplicationDbContext
{
    public SqlServerDbContext(DbContextOptions<SqlServerDbContext> options) : base(options)
    {
    }
}
