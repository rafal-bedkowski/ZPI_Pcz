using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebStore.DAL.Configuration;
using WebStore.DAL.EF;
using WebStore.Services.Configuration;
using WebStore.Services.Interfaces;

namespace WebStore.Tests;

public class ConfigurationTests
{
    private static IServiceCollection Register(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddWebStoreDatabase(configuration, Path.GetTempPath());
        services.AddWebStoreServices();
        return services;
    }

    [Fact]
    public void US_01_01_AllSprintServices_AreRegisteredAndResolvable()
    {
        using var provider = Register(new()).BuildServiceProvider(validateScopes: true);
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IProductService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IOrderService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IInvoiceService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IStoreService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IAddressService>());
    }

    [Fact]
    public void US_01_07_NoProviderConfigured_UsesSqlite()
    {
        using var provider = Register(new()).BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.True(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.IsSqlite());
    }

    [Fact]
    public void US_01_07_SqlServerConfigured_UsesSqlServerWithoutChangingServices()
    {
        var settings = new Dictionary<string, string?>
        {
            ["Database:Provider"] = "SqlServer",
            ["ConnectionStrings:SqlServer"] = "Server=localhost;Database=Ignored"
        };
        using var provider = Register(settings).BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.True(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.IsSqlServer());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IProductService>());
    }

    [Fact]
    public void US_01_07_SqlServerWithoutConnectionString_FailsWithClearMessage()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            Register(new() { ["Database:Provider"] = "SqlServer" }));

        Assert.Contains("ConnectionStrings:SqlServer", ex.Message);
    }

    [Fact]
    public void US_01_07_UnsupportedProvider_FailsListingSupportedValues()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            Register(new() { ["Database:Provider"] = "Oracle" }));

        Assert.Contains("Oracle", ex.Message);
        Assert.Contains("SQLite", ex.Message);
        Assert.Contains("SQL Server", ex.Message);
    }
}
