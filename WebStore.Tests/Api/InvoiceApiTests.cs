using System.Net;
using System.Net.Http.Json;
using WebStore.Tests.Infrastructure;
using WebStore.ViewModels.VM;

namespace WebStore.Tests.Api;

public class InvoiceApiTests : ApiTestBase
{
    private const int MonitorId = TestData.MonitorId;
    private const int MouseId = TestData.MouseId;

    private static AddOrUpdateInvoiceVm NewInvoice(int orderId, int? id = null, string number = "FV/1") => new()
    {
        Id = id, OrderId = orderId, InvoiceNumber = number,
        IssueDate = new DateTime(2026, 1, 12), PaymentDueDate = new DateTime(2026, 1, 26)
    };

    private async Task<InvoiceVm> CreateInvoiceAsync(int orderId, string number = "FV/1")
    {
        var response = await Client.PostAsJsonAsync("/api/InvoiceApi", NewInvoice(orderId, null, number), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await ReadAsync<InvoiceVm>(response);
    }

    [Fact]
    public async Task GetAll_ReturnsEveryInvoice()
    {
        var first = await CreateOrderAsync((MonitorId, 1));
        var second = await CreateOrderAsync((MouseId, 1));
        await CreateInvoiceAsync(first.Id, "FV/1");
        await CreateInvoiceAsync(second.Id, "FV/2");

        var response = await Client.GetAsync("/api/InvoiceApi", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, (await ReadAsync<List<InvoiceVm>>(response)).Count);
    }

    [Fact]
    public async Task GetById_ReturnsInvoiceOrNotFound()
    {
        var invoice = await CreateInvoiceAsync((await CreateOrderAsync((MonitorId, 1))).Id);

        var found = await Client.GetAsync($"/api/InvoiceApi/{invoice.Id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, found.StatusCode);
        Assert.Equal("FV/1", (await ReadAsync<InvoiceVm>(found)).InvoiceNumber);

        Assert.Equal(HttpStatusCode.NotFound, (await Client.GetAsync("/api/InvoiceApi/999", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Client.GetAsync("/api/InvoiceApi/0", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Client.GetAsync("/api/InvoiceApi/abc", TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Post_CreatesInvoiceWithOrderTotalAndReturns201WithLocation()
    {
        var order = await CreateOrderAsync((MonitorId, 2), (MouseId, 1));

        var response = await Client.PostAsJsonAsync("/api/InvoiceApi", NewInvoice(order.Id), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await ReadAsync<InvoiceVm>(response);
        Assert.Equal(order.TotalAmount, created.TotalAmount);
        Assert.Equal(order.Id, created.OrderId);
        Assert.EndsWith($"/api/InvoiceApi/{created.Id}", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Post_WithId_Returns400()
    {
        var order = await CreateOrderAsync((MonitorId, 1));

        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PostAsJsonAsync("/api/InvoiceApi", NewInvoice(order.Id, 5), TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(0, Factory.WithDb(db => db.Invoices.Count()));
    }

    [Fact]
    public async Task Post_SecondInvoiceForTheSameOrder_Returns409AndSavesNothing()
    {
        var order = await CreateOrderAsync((MonitorId, 1));
        await CreateInvoiceAsync(order.Id);

        var response = await Client.PostAsJsonAsync("/api/InvoiceApi", NewInvoice(order.Id, null, "FV/2"), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(1, Factory.WithDb(db => db.Invoices.Count()));
    }

    [Fact]
    public async Task Write_UnknownOrder_Returns400AndSavesNothing()
    {
        var order = await CreateOrderAsync((MonitorId, 1));
        var invoice = await CreateInvoiceAsync(order.Id);

        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PostAsJsonAsync("/api/InvoiceApi", NewInvoice(999), TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PutAsJsonAsync("/api/InvoiceApi", NewInvoice(999, invoice.Id), TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(1, Factory.WithDb(db => db.Invoices.Count()));
    }

    [Fact]
    public async Task Put_UpdatesInvoice_UnknownIs404_MissingIdIs400()
    {
        var order = await CreateOrderAsync((MonitorId, 1));
        var invoice = await CreateInvoiceAsync(order.Id);

        var updated = await Client.PutAsJsonAsync("/api/InvoiceApi", NewInvoice(order.Id, invoice.Id, "FV/99"), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        Assert.Equal("FV/99", (await ReadAsync<InvoiceVm>(updated)).InvoiceNumber);
        Assert.Equal(1, Factory.WithDb(db => db.Invoices.Count()));

        Assert.Equal(HttpStatusCode.NotFound, (await Client.PutAsJsonAsync("/api/InvoiceApi", NewInvoice(order.Id, 999), TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PutAsJsonAsync("/api/InvoiceApi", NewInvoice(order.Id), TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Write_WithInvalidBody_Returns400AndSavesNothing()
    {
        var order = await CreateOrderAsync((MonitorId, 1));

        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PostAsJsonAsync("/api/InvoiceApi", NewInvoice(order.Id, null, ""), TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PostAsync("/api/InvoiceApi", RawJson("{zly json"), TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PutAsync("/api/InvoiceApi", RawJson(""), TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(0, Factory.WithDb(db => db.Invoices.Count()));
    }

    [Fact]
    public async Task EveryRequest_WithoutToken_Returns401()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await Anonymous.GetAsync("/api/InvoiceApi", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await Anonymous.PostAsJsonAsync("/api/InvoiceApi", NewInvoice(1), TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await Anonymous.PutAsJsonAsync("/api/InvoiceApi", NewInvoice(1, 1), TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Delete_IsNotAvailable()
    {
        Assert.Equal(HttpStatusCode.MethodNotAllowed, (await Client.DeleteAsync("/api/InvoiceApi/1", TestContext.Current.CancellationToken)).StatusCode);
    }
}
