using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WebStore.Tests.Infrastructure;
using WebStore.ViewModels.VM;

namespace WebStore.Tests.Api;

public class ProductApiTests : ApiTestBase
{
    [Fact]
    public async Task GetAll_ReturnsAllVisibleProducts()
    {
        var response = await Client.GetAsync("/api/ProductApi", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        var products = await ReadAsync<List<ProductVm>>(response);
        Assert.Equal(Factory.WithDb(db => db.Products.Count()), products.Count);
    }

    [Fact]
    public async Task GetAll_WhenEveryProductIsHidden_ReturnsEmptyList()
    {
        await Client.DeleteAsync($"/api/ProductApi/{TestData.MonitorId}", TestContext.Current.CancellationToken);
        await Client.DeleteAsync($"/api/ProductApi/{TestData.MouseId}", TestContext.Current.CancellationToken);

        var response = await Client.GetAsync("/api/ProductApi", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(await ReadAsync<List<ProductVm>>(response));
    }

    [Fact]
    public async Task GetById_ReturnsProduct()
    {
        var response = await Client.GetAsync($"/api/ProductApi/{TestData.MonitorId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var product = await ReadAsync<ProductVm>(response);
        Assert.Equal(TestData.MonitorId, product.Id);
        Assert.Equal("Monitor Dell 32", product.Name);
    }

    [Theory]
    [InlineData("999")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("abc")]
    public async Task GetById_UnknownOrInvalidId_Returns404(string id)
    {
        Assert.Equal(HttpStatusCode.NotFound, (await Client.GetAsync($"/api/ProductApi/{id}", TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Get_WithoutValidToken_Returns401()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await Anonymous.GetAsync("/api/ProductApi", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await Anonymous.GetAsync("/api/ProductApi/1", TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Post_CreatesProductAndReturns201WithLocation()
    {
        var response = await Client.PostAsJsonAsync("/api/ProductApi", NewProduct(name: "Klawiatura", price: 250m), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await ReadAsync<ProductVm>(response);
        Assert.Equal("Klawiatura", created.Name);
        Assert.Equal(250m, created.Price);
        Assert.EndsWith($"/api/ProductApi/{created.Id}", response.Headers.Location!.ToString());
        Assert.Equal(3, Factory.WithDb(db => db.Products.Count()));
    }

    [Fact]
    public async Task Post_WithZeroId_IsTreatedAsNewProduct()
    {
        var response = await Client.PostAsJsonAsync("/api/ProductApi", NewProduct(id: 0), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithId_Returns400AndSavesNothing()
    {
        var response = await Client.PostAsJsonAsync("/api/ProductApi", NewProduct(id: TestData.MonitorId), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(2, Factory.WithDb(db => db.Products.Count()));
    }

    [Fact]
    public async Task Put_UpdatesExistingProduct()
    {
        var response = await Client.PutAsJsonAsync("/api/ProductApi", NewProduct(TestData.MonitorId, "Monitor Dell 32", 2000m), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2000m, (await ReadAsync<ProductVm>(response)).Price);
        Assert.Equal(2, Factory.WithDb(db => db.Products.Count()));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    public async Task Put_WithoutId_Returns400AndSavesNothing(int? id)
    {
        var response = await Client.PutAsJsonAsync("/api/ProductApi", NewProduct(id), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(2, Factory.WithDb(db => db.Products.Count()));
    }

    [Fact]
    public async Task Put_UnknownId_Returns404AndAddsNothing()
    {
        var response = await Client.PutAsJsonAsync("/api/ProductApi", NewProduct(999), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(2, Factory.WithDb(db => db.Products.Count()));
    }

    [Fact]
    public async Task Put_HiddenProduct_Returns404()
    {
        await Client.DeleteAsync($"/api/ProductApi/{TestData.MonitorId}", TestContext.Current.CancellationToken);

        var response = await Client.PutAsJsonAsync("/api/ProductApi", NewProduct(TestData.MonitorId), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    public async Task Write_WithMissingRequiredField_Returns400WithValidationDetails(string method)
    {
        var id = method == "PUT" ? TestData.MonitorId.ToString() : "null";
        var body = $$"""{"id": {{id}}, "name": "", "description": "opis", "price": 10, "weight": 1, "categoryId": 1, "supplierId": 1}""";

        var response = await Client.SendAsync(new HttpRequestMessage(new HttpMethod(method), "/api/ProductApi") { Content = RawJson(body) }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var text = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("Name", text, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(2, Factory.WithDb(db => db.Products.Count()));
        Assert.Equal("Monitor Dell 32", Factory.WithDb(db => db.Products.Single(p => p.Id == TestData.MonitorId).Name));
    }

    [Theory]
    [InlineData("POST", "")]
    [InlineData("PUT", "")]
    [InlineData("POST", "{niepoprawny json")]
    [InlineData("PUT", "{niepoprawny json")]
    public async Task Write_WithEmptyOrMalformedBody_Returns400(string method, string body)
    {
        var response = await Client.SendAsync(new HttpRequestMessage(new HttpMethod(method), "/api/ProductApi") { Content = RawJson(body) }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(2, Factory.WithDb(db => db.Products.Count()));
    }

    [Fact]
    public async Task Post_UnknownCategoryOrSupplier_Returns400WithMessageAndSavesNothing()
    {
        var unknownCategory = NewProduct();
        unknownCategory.CategoryId = 999;
        var unknownSupplier = NewProduct();
        unknownSupplier.SupplierId = 999;

        foreach (var model in new[] { unknownCategory, unknownSupplier })
        {
            var response = await Client.PostAsJsonAsync("/api/ProductApi", model, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("999", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        }

        Assert.Equal(2, Factory.WithDb(db => db.Products.Count()));
    }

    [Fact]
    public async Task Put_UnknownCategory_Returns400()
    {
        var model = NewProduct(TestData.MonitorId);
        model.CategoryId = 999;

        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PutAsJsonAsync("/api/ProductApi", model, TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Put_WithoutImage_KeepsExistingImage()
    {
        var image = new byte[] { 1, 2, 3, 4 };
        var model = NewProduct(TestData.MonitorId, "Monitor Dell 32", 1000m);
        model.ImageBytes = image;
        Assert.Equal(HttpStatusCode.OK, (await Client.PutAsJsonAsync("/api/ProductApi", model, TestContext.Current.CancellationToken)).StatusCode);

        var response = await Client.PutAsJsonAsync("/api/ProductApi", NewProduct(TestData.MonitorId, "Monitor Dell 32", 1100m), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var product = await ReadAsync<ProductVm>(await Client.GetAsync($"/api/ProductApi/{TestData.MonitorId}", TestContext.Current.CancellationToken));
        Assert.Equal(image, product.ImageBytes);
        Assert.Equal(1100m, product.Price);
    }

    [Fact]
    public async Task Price_WithFractionalPart_IsStoredExactly()
    {
        var created = await CreateProductAsync(price: 19.99m);

        var fetched = await ReadAsync<ProductVm>(await Client.GetAsync($"/api/ProductApi/{created.Id}", TestContext.Current.CancellationToken));

        Assert.Equal(19.99m, fetched.Price);
    }

    [Fact]
    public async Task Write_WithoutValidToken_Returns401AndSavesNothing()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await Anonymous.PostAsJsonAsync("/api/ProductApi", NewProduct(), TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await Anonymous.PutAsJsonAsync("/api/ProductApi", NewProduct(TestData.MonitorId, price: 1m), TestContext.Current.CancellationToken)).StatusCode);

        Assert.Equal(2, Factory.WithDb(db => db.Products.Count()));
        Assert.Equal(1000m, Factory.WithDb(db => db.Products.Single(p => p.Id == TestData.MonitorId).Price));
    }

    [Fact]
    public async Task Delete_HidesProductButKeepsRecord()
    {
        var response = await Client.DeleteAsync($"/api/ProductApi/{TestData.MonitorId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(0, response.Content.Headers.ContentLength ?? 0);
        Assert.Equal(HttpStatusCode.NotFound, (await Client.GetAsync($"/api/ProductApi/{TestData.MonitorId}", TestContext.Current.CancellationToken)).StatusCode);
        var visible = await ReadAsync<List<ProductVm>>(await Client.GetAsync("/api/ProductApi", TestContext.Current.CancellationToken));
        Assert.DoesNotContain(visible, p => p.Id == TestData.MonitorId);
        Assert.True(Factory.WithDb(db => db.Products.Single(p => p.Id == TestData.MonitorId).IsHidden));
    }

    [Fact]
    public async Task Delete_UnknownOrAlreadyHiddenProduct_Returns404()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await Client.DeleteAsync("/api/ProductApi/999", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await Client.DeleteAsync($"/api/ProductApi/{TestData.MouseId}", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Client.DeleteAsync($"/api/ProductApi/{TestData.MouseId}", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(2, Factory.WithDb(db => db.Products.Count()));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-3")]
    [InlineData("abc")]
    public async Task Delete_InvalidId_Returns404(string id)
    {
        Assert.Equal(HttpStatusCode.NotFound, (await Client.DeleteAsync($"/api/ProductApi/{id}", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(0, Factory.WithDb(db => db.Products.Count(p => p.IsHidden)));
    }

    [Fact]
    public async Task Delete_WithoutValidToken_Returns401AndKeepsProductVisible()
    {
        var response = await Anonymous.DeleteAsync($"/api/ProductApi/{TestData.MonitorId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.False(Factory.WithDb(db => db.Products.Single(p => p.Id == TestData.MonitorId).IsHidden));
    }

    [Fact]
    public async Task Delete_ProductInExistingOrder_LeavesOrderUntouched()
    {
        var order = await CreateOrderAsync((TestData.MonitorId, 2), (TestData.MouseId, 1));

        Assert.Equal(HttpStatusCode.NoContent, (await Client.DeleteAsync($"/api/ProductApi/{TestData.MonitorId}", TestContext.Current.CancellationToken)).StatusCode);

        var response = await Client.GetAsync($"/api/OrderApi/{order.Id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fetched = await ReadAsync<OrderVm>(response);
        Assert.Equal(2500m, fetched.TotalAmount);
        Assert.Equal(2, fetched.Products.Count);
        Assert.Equal(1000m, fetched.Products.Single(p => p.ProductId == TestData.MonitorId).UnitPrice);
    }

    [Fact]
    public async Task Order_WithHiddenProduct_IsRejectedWithValidationError()
    {
        await Client.DeleteAsync($"/api/ProductApi/{TestData.MonitorId}", TestContext.Current.CancellationToken);

        var post = await Client.PostAsJsonAsync("/api/OrderApi", NewOrder(null, TestData.CustomerId, (TestData.MonitorId, 1)), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, post.StatusCode);
        Assert.Equal(0, Factory.WithDb(db => db.Orders.Count()));
    }

    [Fact]
    public async Task Service_UnexpectedException_Returns500WithGenericMessageAndIsLogged()
    {
        using var factory = new ApiFactory
        {
            ConfigureServices = services => services.AddScoped<WebStore.Services.Interfaces.IProductService, ThrowingProductService>()
        };
        using var client = factory.CreateAuthorizedClient();

        var response = await client.GetAsync("/api/ProductApi", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.DoesNotContain(ThrowingProductService.Secret, body);
        Assert.DoesNotContain("   at ", body);
        Assert.Contains(factory.Logs.Entries, e => e.Level == Microsoft.Extensions.Logging.LogLevel.Error
                                                   && e.Exception is InvalidOperationException ex
                                                   && ex.Message.Contains(ThrowingProductService.Secret));
    }
}
