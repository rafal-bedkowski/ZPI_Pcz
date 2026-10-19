using Microsoft.EntityFrameworkCore;

namespace WebStore.DAL.EF;

// Osobny kontekst dla migracji SQLite: migracje obu providerów nie mogą się mieszać w jednym zestawie.
public class SqliteDbContext : ApplicationDbContext
{
    public SqliteDbContext(DbContextOptions<SqliteDbContext> options) : base(options)
    {
    }
}
