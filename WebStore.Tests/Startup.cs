using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebStore.DAL.EF;
using WebStore.Model.DataModels;
using WebStore.Services.Configuration;
using WebStore.Tests.Infrastructure;

namespace WebStore.Tests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging();

        // Każdy test dostaje własne połączenie SQLite in-memory (baza istnieje, dopóki połączenie jest otwarte)
        // wypełnione danymi startowymi (SeedData).
        services.AddScoped(_ =>
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            return connection;
        });
        services.SeedData();

        services.AddIdentityCore<User>(o =>
            {
                o.Password.RequiredLength = 6;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredUniqueChars = 0;
                o.SignIn.RequireConfirmedAccount = false;
            })
            .AddRoles<IdentityRole<int>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddWebStoreServices();

        services.AddScoped<TestLogger>();
        services.AddScoped<ILogger>(sp => sp.GetRequiredService<TestLogger>());
    }
}
