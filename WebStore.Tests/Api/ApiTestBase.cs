using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using WebStore.Tests.Infrastructure;
using WebStore.ViewModels.VM;

namespace WebStore.Tests.Api;

// Każdy test (osobna instancja klasy) dostaje własną aplikację i własną bazę, więc testy są od siebie niezależne.
public abstract class ApiTestBase : IDisposable
{
    protected static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    protected ApiTestBase(ApiFactory? factory = null)
    {
        Factory = factory ?? new ApiFactory();
        Client = Factory.CreateAuthorizedClient();
    }

    protected ApiFactory Factory { get; }

    // Klient z poprawnym tokenem.
    protected HttpClient Client { get; }

    // Klient bez tokenu.
    protected HttpClient Anonymous => Factory.CreateApiClient();

    public void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
    }

    protected static StringContent RawJson(string json) => new(json, Encoding.UTF8, "application/json");

    protected async Task<T> ReadAsync<T>(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<T>(Json, TestContext.Current.CancellationToken))!;

    protected async Task<ProductVm> CreateProductAsync(string name = "Klawiatura", decimal price = 250m)
    {
        var response = await Client.PostAsJsonAsync("/api/ProductApi", NewProduct(name: name, price: price), TestContext.Current.CancellationToken);
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        return await ReadAsync<ProductVm>(response);
    }

    protected static AddOrUpdateProductVm NewProduct(int? id = null, string name = "Klawiatura", decimal price = 250m) => new()
    {
        Id = id, Name = name, Description = "opis", Price = price, Weight = 1.2f,
        CategoryId = TestData.CategoryId, SupplierId = TestData.SupplierId
    };

    protected static AddOrUpdateOrderVm NewOrder(int? id = null, int customerId = TestData.CustomerId,
        params (int ProductId, int Quantity)[] items) => new()
    {
        Id = id, CustomerId = customerId, OrderDate = new DateTime(2026, 1, 10), DeliveryDate = new DateTime(2026, 1, 15),
        TrackingNumber = 123456789,
        Products = items.Select(i => new AddOrUpdateOrderProductVm { ProductId = i.ProductId, Quantity = i.Quantity }).ToList()
    };

    protected async Task<OrderVm> CreateOrderAsync(params (int ProductId, int Quantity)[] items)
    {
        var response = await Client.PostAsJsonAsync("/api/OrderApi", NewOrder(null, TestData.CustomerId, items), TestContext.Current.CancellationToken);
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        return await ReadAsync<OrderVm>(response);
    }

    protected async Task<int> CountAsync(string url) =>
        (await ReadAsync<List<System.Text.Json.Nodes.JsonObject>>(await Client.GetAsync(url, TestContext.Current.CancellationToken))).Count;
}
