using System.Net;
using System.Net.Http.Json;
using WebStore.Tests.Infrastructure;
using WebStore.ViewModels.VM;

namespace WebStore.Tests.Api;

// Uruchamiane tylko, gdy zmienna WEBSTORE_TEST_SQLSERVER wskazuje działającą instancję SQL Server;
// w przeciwnym razie są pomijane. Każdy test tworzy aplikację z własną bazą, usuwaną po teście.
public sealed class SqlServerApiTests : IDisposable
{
    private const string ConnectionVariable = "WEBSTORE_TEST_SQLSERVER";
    private ApiFactory? _factory;

    private ApiFactory Open()
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionVariable);
        Assert.SkipUnless(!string.IsNullOrWhiteSpace(connectionString), $"Brak zmiennej {ConnectionVariable}.");

        _factory = new ApiFactory { SqlServerConnectionString = connectionString };
        return _factory;
    }

    [Fact]
    public async Task SqlServer_LoginIssuesTokenAcceptedByProtectedEndpoint()
    {
        var factory = Open();
        var token = await factory.LoginAsync(ApiFactory.AdminLogin, factory.TestAccountPassword!);
        using var client = factory.CreateApiClient();

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/ProductApi", TestContext.Current.CancellationToken)).StatusCode);

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await client.GetAsync("/api/ProductApi", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, (await response.Content.ReadFromJsonAsync<List<ProductVm>>(TestContext.Current.CancellationToken))!.Count);
    }

    [Fact]
    public async Task SqlServer_Price_WithFractionalPart_IsStoredExactly()
    {
        using var client = Open().CreateAuthorizedClient();
        var product = new AddOrUpdateProductVm
        {
            Name = "Klawiatura", Description = "opis", Price = 19.99m, Weight = 1.2f,
            CategoryId = TestData.CategoryId, SupplierId = TestData.SupplierId
        };

        var created = await client.PostAsJsonAsync("/api/ProductApi", product, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var id = (await created.Content.ReadFromJsonAsync<ProductVm>(TestContext.Current.CancellationToken))!.Id;

        var fetched = await client.GetFromJsonAsync<ProductVm>($"/api/ProductApi/{id}", TestContext.Current.CancellationToken);

        Assert.Equal(19.99m, fetched!.Price);
    }

    public void Dispose() => _factory?.Dispose();
}
