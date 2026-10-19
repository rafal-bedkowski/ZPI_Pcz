using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebStore.DAL.EF;

// Fabryki używane wyłącznie przez narzędzie dotnet ef. Plik SQLite powstaje w bieżącym katalogu
// (przy poleceniach z projektem startowym WebStore.Web jest to katalog tego projektu).
public class SqliteDbContextFactory : IDesignTimeDbContextFactory<SqliteDbContext>
{
    public SqliteDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SqliteDbContext>()
            .UseSqlite("Data Source=webstore.db")
            .Options;
        return new SqliteDbContext(options);
    }
}

public class SqlServerDbContextFactory : IDesignTimeDbContextFactory<SqlServerDbContext>
{
    public const string ConnectionVariable = "ConnectionStrings__SqlServer";

    public SqlServerDbContext CreateDbContext(string[] args)
    {
        // Generowanie migracji nie łączy się z bazą, więc bez zmiennej używany jest neutralny ciąg bez haseł.
        var connectionString = Environment.GetEnvironmentVariable(ConnectionVariable)
                               ?? "Server=(localdb)\\MSSQLLocalDB;Database=WebStore;Trusted_Connection=True;";
        var options = new DbContextOptionsBuilder<SqlServerDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new SqlServerDbContext(options);
    }
}
