using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebStore.DAL.EF;

namespace WebStore.DAL.Configuration;

public static class DatabaseServiceCollectionExtensions
{
    public const string SqliteProvider = "Sqlite";
    public const string SqlServerProvider = "SqlServer";
    public const string ProviderKey = "Database:Provider";
    public const string SqliteFileKey = "Database:SqliteFile";
    public const string SqlServerConnectionName = "SqlServer";
    public const string DefaultSqliteFile = "webstore.db";

    /// <summary>
    /// Rejestruje <see cref="ApplicationDbContext"/> na providerze wskazanym w konfiguracji. Docelową bazą jest SQL Server;
    /// SQLite trzeba wybrać jawnie, a brak ustawienia providera przerywa start. Ciąg połączenia SQL Server pochodzi
    /// wyłącznie z user-secrets lub zmiennej środowiskowej.
    /// </summary>
    public static IServiceCollection AddWebStoreDatabase(
        this IServiceCollection services, IConfiguration configuration, string contentRootPath)
    {
        var provider = configuration[ProviderKey];
        if (string.IsNullOrWhiteSpace(provider))
        {
            throw new InvalidOperationException(
                $"Nie skonfigurowano bazy danych: brakuje ustawienia '{ProviderKey}'. " +
                $"Ustaw je na {SqlServerProvider} (SQL Server, razem z ciągiem połączenia 'ConnectionStrings:SqlServer') " +
                $"albo na {SqliteProvider} (SQLite, np. na macOS) w user-secrets " +
                $"(dotnet user-secrets set \"{ProviderKey}\" <wartość> --project WebStore.Web) " +
                $"lub w zmiennej środowiskowej {ProviderKey.Replace(":", "__")}.");
        }

        if (string.Equals(provider, SqliteProvider, StringComparison.OrdinalIgnoreCase))
        {
            var file = configuration[SqliteFileKey];
            var path = Path.Combine(contentRootPath, string.IsNullOrWhiteSpace(file) ? DefaultSqliteFile : file);
            services.AddDbContext<SqliteDbContext>(o => o.UseSqlite($"Data Source={path}"));
            services.AddScoped<ApplicationDbContext>(sp => sp.GetRequiredService<SqliteDbContext>());
        }
        else if (string.Equals(provider, SqlServerProvider, StringComparison.OrdinalIgnoreCase))
        {
            var connectionString = configuration.GetConnectionString(SqlServerConnectionName);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Wybrano provider SQL Server, ale brakuje ciągu połączenia 'ConnectionStrings:SqlServer'. " +
                    "Ustaw go w user-secrets lub zmiennej środowiskowej ConnectionStrings__SqlServer.");
            }

            services.AddDbContext<SqlServerDbContext>(o => o.UseSqlServer(connectionString));
            services.AddScoped<ApplicationDbContext>(sp => sp.GetRequiredService<SqlServerDbContext>());
        }
        else
        {
            throw new InvalidOperationException(
                $"Nieobsługiwany provider bazy danych '{provider}' w ustawieniu '{ProviderKey}'. " +
                $"Obsługiwane wartości: {SqliteProvider} (SQLite), {SqlServerProvider} (SQL Server).");
        }

        return services;
    }
}
