using System.Net;
using System.Net.Http.Json;
using WebStore.Tests.Infrastructure;
using WebStore.ViewModels.VM;

namespace WebStore.Tests.Api;

public class OrderApiTests : ApiTestBase
{
    [Fact]
    public async Task GetAll_ReturnsEveryOrder()
    {
        await CreateOrderAsync((TestData.MonitorId, 1));
        await CreateOrderAsync((TestData.MouseId, 1));

        var response = await Client.GetAsync("/api/OrderApi", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, (await ReadAsync<List<OrderVm>>(response)).Count);
    }

    [Fact]
    public async Task GetAll_ForCustomerToken_IsNotFilteredByRole()
    {
        await CreateOrderAsync((TestData.MonitorId, 1));
        using var customer = Factory.CreateAuthorizedClient(ApiFactory.CustomerLogin, "Customer");

        var response = await customer.GetAsync("/api/OrderApi", TestContext.Current.CancellationToken);

        Assert.Single(await ReadAsync<List<OrderVm>>(response));
    }

    [Fact]
    public async Task GetById_ReturnsOrderWithItemsAndTotal()
    {
        var order = await CreateOrderAsync((TestData.MonitorId, 2), (TestData.MouseId, 1));

        var response = await Client.GetAsync($"/api/OrderApi/{order.Id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fetched = await ReadAsync<OrderVm>(response);
        Assert.Equal(2, fetched.Products.Count);
        Assert.Equal(2500m, fetched.TotalAmount);
    }

    [Theory]
    [InlineData("999")]
    [InlineData("0")]
    [InlineData("abc")]
    public async Task GetById_UnknownOrInvalidId_Returns404(string id)
    {
        Assert.Equal(HttpStatusCode.NotFound, (await Client.GetAsync($"/api/OrderApi/{id}", TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Post_CreatesOrderWithFrozenPricesAndReturns201WithLocation()
    {
        var response = await Client.PostAsJsonAsync("/api/OrderApi", NewOrder(null, TestData.CustomerId, (TestData.MonitorId, 2), (TestData.MouseId, 1)), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await ReadAsync<OrderVm>(response);
        Assert.Equal(2500m, created.TotalAmount);
        Assert.EndsWith($"/api/OrderApi/{created.Id}", response.Headers.Location!.ToString());

        await Client.PutAsJsonAsync("/api/ProductApi", NewProduct(TestData.MonitorId, "Monitor Dell 32", 5000m), TestContext.Current.CancellationToken);
        var fetched = await ReadAsync<OrderVm>(await Client.GetAsync($"/api/OrderApi/{created.Id}", TestContext.Current.CancellationToken));
        Assert.Equal(2500m, fetched.TotalAmount);
        Assert.Equal(1000m, fetched.Products.Single(p => p.ProductId == TestData.MonitorId).UnitPrice);
    }

    [Fact]
    public async Task Post_WithId_Returns400()
    {
        var response = await Client.PostAsJsonAsync("/api/OrderApi", NewOrder(5, TestData.CustomerId, (TestData.MonitorId, 1)), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, Factory.WithDb(db => db.Orders.Count()));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(1, -2)]
    [InlineData(-1, 1)]
    [InlineData(999, 1)]
    public async Task Write_WithInvalidItem_Returns400AndSavesNothing(int productId, int quantity)
    {
        var post = await Client.PostAsJsonAsync("/api/OrderApi", NewOrder(null, TestData.CustomerId, (productId, quantity)), TestContext.Current.CancellationToken);
        var order = await CreateOrderAsync((TestData.MouseId, 1));
        var put = await Client.PutAsJsonAsync("/api/OrderApi", NewOrder(order.Id, TestData.CustomerId, (productId, quantity)), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, post.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, put.StatusCode);
        Assert.Equal(1, Factory.WithDb(db => db.Orders.Count()));
        Assert.Equal(1, Factory.WithDb(db => db.OrderProducts.Count()));
    }

    [Fact]
    public async Task Write_WithEmptyItemList_Returns400WithMessage()
    {
        var response = await Client.PostAsJsonAsync("/api/OrderApi", NewOrder(null, TestData.CustomerId), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("pozycj", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0, Factory.WithDb(db => db.Orders.Count()));
    }

    [Fact]
    public async Task Write_UnknownCustomer_Returns400AndSavesNothing()
    {
        var post = await Client.PostAsJsonAsync("/api/OrderApi", NewOrder(null, 999, (TestData.MonitorId, 1)), TestContext.Current.CancellationToken);
        var order = await CreateOrderAsync((TestData.MouseId, 1));
        var put = await Client.PutAsJsonAsync("/api/OrderApi", NewOrder(order.Id, 999, (TestData.MonitorId, 1)), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, post.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, put.StatusCode);
        Assert.Equal(1, Factory.WithDb(db => db.Orders.Count()));
    }

    [Fact]
    public async Task Put_UpdatesOrder_UnknownIs404_MissingIdIs400()
    {
        var order = await CreateOrderAsync((TestData.MouseId, 1));

        var updated = await Client.PutAsJsonAsync("/api/OrderApi", NewOrder(order.Id, TestData.CustomerId, (TestData.MouseId, 4)), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        Assert.Equal(2000m, (await ReadAsync<OrderVm>(updated)).TotalAmount);

        Assert.Equal(HttpStatusCode.NotFound,
            (await Client.PutAsJsonAsync("/api/OrderApi", NewOrder(999, TestData.CustomerId, (TestData.MouseId, 1)), TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await Client.PutAsJsonAsync("/api/OrderApi", NewOrder(null, TestData.CustomerId, (TestData.MouseId, 1)), TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(1, Factory.WithDb(db => db.Orders.Count()));
    }

    [Fact]
    public async Task Write_WithEmptyOrMalformedBody_Returns400()
    {
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PostAsync("/api/OrderApi", RawJson(""), TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PutAsync("/api/OrderApi", RawJson("{zly json"), TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task EveryRequest_WithoutToken_Returns401()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await Anonymous.GetAsync("/api/OrderApi", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await Anonymous.GetAsync("/api/OrderApi/1", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await Anonymous.PostAsJsonAsync("/api/OrderApi", NewOrder(null, TestData.CustomerId, (TestData.MonitorId, 1)), TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(0, Factory.WithDb(db => db.Orders.Count()));
    }

    [Fact]
    public async Task Delete_IsNotAvailable()
    {
        var order = await CreateOrderAsync((TestData.MonitorId, 1));

        Assert.Equal(HttpStatusCode.MethodNotAllowed, (await Client.DeleteAsync($"/api/OrderApi/{order.Id}", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(1, Factory.WithDb(db => db.Orders.Count()));
    }
}
