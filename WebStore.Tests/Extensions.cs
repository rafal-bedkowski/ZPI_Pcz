using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebStore.DAL.EF;
using WebStore.Tests.Infrastructure;

namespace WebStore.Tests;

public static class Extensions
{
    // Rejestruje kontekst bazy wypełniony danymi startowymi z TestData (jedno źródło danych). Kontekst jest
    // tworzony per zakres (test) na połączeniu SQLite in-memory zarejestrowanym w tym samym zakresie, więc
    // zmiany z jednego testu nie są widoczne w innych.
    public static IServiceCollection SeedData(this IServiceCollection services)
    {
        return services.AddScoped(sp =>
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(sp.GetRequiredService<SqliteConnection>())
                .Options;
            var db = new ApplicationDbContext(options);
            db.Database.EnsureCreated();
            TestData.Seed(db);
            return db;
        });
    }
}
