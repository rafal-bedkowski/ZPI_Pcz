using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebStore.Tests.Infrastructure;
using WebStore.Model.DataModels;
using WebStore.Web.Services;

namespace WebStore.Tests.Api;

public class TestAccountSeedingTests
{
    private static List<string> TestAccounts(ApiFactory factory) => factory.WithDb(db =>
        db.Users.Where(u => u.UserName!.EndsWith("@webstore.local")).Select(u => u.UserName!).OrderBy(n => n).ToList());

    [Fact]
    public async Task Development_WithPassword_CreatesRolesAndAccounts()
    {
        using var factory = new ApiFactory();
        _ = factory.CreateClient();

        Assert.Equal(new[] { ApiFactory.AdminLogin, ApiFactory.CustomerLogin }, TestAccounts(factory));
        factory.WithDb(db =>
        {
            Assert.Equal(new[] { "Admin", "Customer" }, db.Roles.Select(r => r.Name!).OrderBy(n => n).ToList());
            Assert.IsType<Customer>(db.Users.Single(u => u.UserName == ApiFactory.CustomerLogin));
            Assert.IsNotType<Customer>(db.Users.Single(u => u.UserName == ApiFactory.AdminLogin));
        });

        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var admin = (await users.FindByNameAsync(ApiFactory.AdminLogin))!;
        var customer = (await users.FindByNameAsync(ApiFactory.CustomerLogin))!;
        Assert.Equal(new[] { "Admin" }, await users.GetRolesAsync(admin));
        Assert.Equal(new[] { "Customer" }, await users.GetRolesAsync(customer));
        Assert.True(await users.CheckPasswordAsync(admin, factory.TestAccountPassword!));
        Assert.True(await users.CheckPasswordAsync(customer, factory.TestAccountPassword!));
    }

    [Fact]
    public async Task SeedingAgain_DoesNotCreateDuplicates()
    {
        using var factory = new ApiFactory();
        _ = factory.CreateClient();
        var rolesBefore = factory.WithDb(db => db.Roles.Count());

        using (var scope = factory.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<TestAccountSeeder>().SeedAsync();
        }

        Assert.Equal(new[] { ApiFactory.AdminLogin, ApiFactory.CustomerLogin }, TestAccounts(factory));
        Assert.Equal(rolesBefore, factory.WithDb(db => db.Roles.Count()));
    }

    [Fact]
    public void OutsideDevelopment_AccountsAreNotCreated()
    {
        using var factory = new ApiFactory { EnvironmentName = "Production" };
        _ = factory.CreateClient();

        Assert.Empty(TestAccounts(factory));
        Assert.Equal(0, factory.WithDb(db => db.Roles.Count()));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Development_WithoutPassword_SkipsAccountsAndWarnsInPolish(string? password)
    {
        using var factory = new ApiFactory { TestAccountPassword = password };
        using var client = factory.CreateApiClient();

        Assert.Empty(TestAccounts(factory));
        Assert.Contains(factory.Logs.Entries, e => e.Level == LogLevel.Warning
                                                   && e.Message.Contains("Seed:TestAccountPassword")
                                                   && e.Message.Contains("Nie utworzono kont testowych"));
        var response = await client.PostAsJsonAsync("/api/AccountApi/Login",
            ApiFactory.Credentials(ApiFactory.AdminLogin, "dowolne-haslo"), TestContext.Current.CancellationToken);
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
