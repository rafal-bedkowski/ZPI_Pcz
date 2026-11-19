using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WebStore.Tests.Infrastructure;
using WebStore.ViewModels.VM;

namespace WebStore.Tests.Api;

// Aplikacja z "zbudowanym" klientem: tymczasowy katalog główny z wwwroot/index.html zastępuje wynik npm run build.
public sealed class HostingTests : IDisposable
{
    private const string ClientMarker = "<title>klient-webstore</title>";

    private readonly string _contentRoot = Path.Combine(Path.GetTempPath(), "webstore-tests-" + Guid.NewGuid().ToString("N"));

    public HostingTests()
    {
        Directory.CreateDirectory(Path.Combine(_contentRoot, "wwwroot"));
        File.WriteAllText(Path.Combine(_contentRoot, "wwwroot", "index.html"), $"<html><head>{ClientMarker}</head></html>");
    }

    public void Dispose() => Directory.Delete(_contentRoot, recursive: true);

    private ApiFactory ClientFactory(string environment = "Development") =>
        new() { EnvironmentName = environment, ContentRoot = _contentRoot };

    [Fact]
    public async Task UnknownApiController_Returns404InsteadOfClientPage()
    {
        using var factory = ClientFactory();
        using var client = factory.CreateApiClient();

        foreach (var url in new[] { "/api/NieIstniejacyKontroler", "/api/ProductApi/abc", "/api" })
        {
            var response = await client.GetAsync(url, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.DoesNotContain(ClientMarker, await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        }
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/counter")]
    [InlineData("/dowolna/sciezka/klienta")]
    public async Task ClientRoutes_ServeClientPage(string url)
    {
        using var factory = ClientFactory();
        using var client = factory.CreateApiClient();

        var response = await client.GetAsync(url, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(ClientMarker, await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SwaggerUi_InDevelopment_ShowsDocumentationInsteadOfClient()
    {
        using var factory = ClientFactory();
        using var client = factory.CreateApiClient();

        var response = await client.GetAsync("/swagger/index.html", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("Swagger UI", html);
        Assert.DoesNotContain(ClientMarker, html);
    }

    [Fact]
    public async Task SwaggerDocument_InDevelopment_DescribesEndpointsAndBearerScheme()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateApiClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var root = document.RootElement;
        Assert.StartsWith("3.", root.GetProperty("openapi").GetString());
        Assert.Equal("WebStore API", root.GetProperty("info").GetProperty("title").GetString());
        Assert.Equal("v1", root.GetProperty("info").GetProperty("version").GetString());

        var paths = root.GetProperty("paths");
        foreach (var path in new[]
                 {
                     "/api/AccountApi/Login", "/api/ProductApi", "/api/ProductApi/{id}", "/api/OrderApi", "/api/OrderApi/{id}",
                     "/api/InvoiceApi", "/api/InvoiceApi/{id}", "/api/StoreApi", "/api/StoreApi/{id}", "/api/AddressApi",
                     "/api/AddressApi/{id}"
                 })
        {
            Assert.True(paths.TryGetProperty(path, out _), $"Brak ścieżki {path} w dokumencie OpenAPI.");
        }

        Assert.True(paths.GetProperty("/api/ProductApi/{id}").TryGetProperty("delete", out _));
        var scheme = root.GetProperty("components").GetProperty("securitySchemes").EnumerateObject().Single().Value;
        Assert.Equal("http", scheme.GetProperty("type").GetString());
        Assert.Equal("bearer", scheme.GetProperty("scheme").GetString());
        Assert.Equal("JWT", scheme.GetProperty("bearerFormat").GetString());
        Assert.True(root.GetProperty("components").GetProperty("schemas").TryGetProperty(nameof(ProductVm), out _));
    }

    [Theory]
    [InlineData("/swagger/index.html")]
    [InlineData("/swagger/v1/swagger.json")]
    public async Task Swagger_OutsideDevelopment_Returns404(string url)
    {
        using var factory = ClientFactory("Production");
        using var client = factory.CreateApiClient();

        var response = await client.GetAsync(url, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.DoesNotContain(ClientMarker, await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task HttpRequest_IsRedirectedToHttps()
    {
        using var factory = new ApiFactory { HttpsPort = "7192" };
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost"),
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/api/ProductApi", TestContext.Current.CancellationToken);

        Assert.True(response.StatusCode is HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect);
        Assert.StartsWith("https://localhost:7192/", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task ErrorAndNoContentResponses_HaveExpectedContentTypes()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateAuthorizedClient();

        var error = await client.GetAsync("/api/ProductApi/999", TestContext.Current.CancellationToken);
        var created = await client.PostAsJsonAsync("/api/ProductApi",
            new AddOrUpdateProductVm { Name = "Nowy", Description = "opis", Price = 1m, CategoryId = TestData.CategoryId, SupplierId = TestData.SupplierId },
            TestContext.Current.CancellationToken);
        var deleted = await client.DeleteAsync($"/api/ProductApi/{TestData.MonitorId}", TestContext.Current.CancellationToken);

        Assert.Contains(error.Content.Headers.ContentType?.MediaType, new[] { "application/json", "application/problem+json" });
        Assert.Equal("application/json", created.Content.Headers.ContentType?.MediaType);
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Empty(await deleted.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
    }
}
