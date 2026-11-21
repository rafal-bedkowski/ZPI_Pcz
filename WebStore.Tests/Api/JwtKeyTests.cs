using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebStore.Tests.Infrastructure;
using WebStore.ViewModels.VM;

namespace WebStore.Tests.Api;

public class JwtKeyTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("za-krotki-klucz")]
    public void OutsideDevelopment_MissingOrShortKey_StopsStartupWithPolishMessage(string? key)
    {
        using var factory = new ApiFactory { EnvironmentName = "Production", SecretKey = key };

        var exception = Assert.Throws<OptionsValidationException>(() => factory.CreateClient());

        Assert.Contains("JwtOptions:SecretKey", exception.Message);
        Assert.Contains("klucz", exception.Message);
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public void WithoutDatabaseProvider_StartupFailsWithPolishMessage(string environment)
    {
        using var factory = new ApiFactory { EnvironmentName = environment, DatabaseProvider = null };

        var exception = Assert.Throws<InvalidOperationException>(() => factory.CreateClient());

        Assert.Contains("Nie skonfigurowano bazy danych", exception.Message);
        Assert.Contains("Database:Provider", exception.Message);
    }

    [Fact]
    public void OutsideDevelopment_ConfiguredKey_StartsNormally()
    {
        using var factory = new ApiFactory { EnvironmentName = "Production" };

        using var client = factory.CreateClient();

        Assert.NotNull(client);
    }

    [Fact]
    public async Task Development_WithoutKey_GeneratesRandomKeyAndTokensDoNotSurviveRestart()
    {
        using var first = new ApiFactory { SecretKey = null };
        var token = await first.LoginAsync(ApiFactory.AdminLogin, first.TestAccountPassword!);
        using var firstClient = first.CreateApiClient();
        firstClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Assert.Equal(HttpStatusCode.OK, (await firstClient.GetAsync("/api/ProductApi", TestContext.Current.CancellationToken)).StatusCode);

        using var restarted = new ApiFactory { SecretKey = null };
        using var restartedClient = restarted.CreateApiClient();
        restartedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        Assert.Equal(HttpStatusCode.Unauthorized, (await restartedClient.GetAsync("/api/ProductApi", TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public void Development_WithoutKey_UsesKeyOfAtLeast32Bytes()
    {
        using var factory = new ApiFactory { SecretKey = null };
        _ = factory.CreateClient();

        var options = factory.Services.GetRequiredService<IOptions<JwtOptionsVm>>().Value;

        Assert.True(System.Text.Encoding.UTF8.GetByteCount(options.SecretKey) >= 32);
    }
}
