using Microsoft.EntityFrameworkCore;

namespace WebStore.Tests.Infrastructure;

// Testy SQL Server tworzą i usuwają bazy na lokalnej instancji, gdzie operacje plikowe (CREATE DATABASE) potrafią
// chwilowo trwać znacznie dłużej niż domyślne 30 s; limit obejmuje też EnsureCreated i EnsureDeleted.
public static class SqlServerTestOptions
{
    public const int CommandTimeoutSeconds = 120;

    public static DbContextOptionsBuilder UseSqlServerForTests(this DbContextOptionsBuilder builder, string connectionString)
        => builder.UseSqlServer(connectionString, sql => sql.CommandTimeout(CommandTimeoutSeconds));

    public static DbContextOptionsBuilder<TContext> UseSqlServerForTests<TContext>(
        this DbContextOptionsBuilder<TContext> builder, string connectionString) where TContext : DbContext
        => builder.UseSqlServer(connectionString, sql => sql.CommandTimeout(CommandTimeoutSeconds));
}
