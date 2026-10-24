using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

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
    public SqlServerDbContext CreateDbContext(string[] args)
    {
        // Ciąg połączenia pochodzi z tych samych źródeł co w aplikacji: zmiennej środowiskowej lub user-secrets.
        // Generowanie migracji nie łączy się z bazą, więc bez konfiguracji używany jest neutralny ciąg bez haseł.
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<SqlServerDbContextFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();
        var connectionString = configuration.GetConnectionString("SqlServer")
                               ?? "Server=localhost;Database=WebStore;Trusted_Connection=True;";
        var options = new DbContextOptionsBuilder<SqlServerDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new SqlServerDbContext(options);
    }
}
