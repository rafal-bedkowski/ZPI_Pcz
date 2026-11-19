using System.Net;
using System.Net.Http.Json;
using WebStore.Tests.Infrastructure;
using WebStore.ViewModels.VM;

namespace WebStore.Tests.Api;

public class StoreApiTests : ApiTestBase
{
    private static AddOrUpdateStoreVm NewStore(int? id = null, string name = "Sklep Północny", int addressId = TestData.FreeAddressId) =>
        new() { Id = id, Name = name, AddressId = addressId };

    [Fact]
    public async Task GetAll_ReturnsEveryStore()
    {
        var response = await Client.GetAsync("/api/StoreApi", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(Factory.WithDb(db => db.StationaryStores.Count()), (await ReadAsync<List<StoreVm>>(response)).Count);
    }

    [Fact]
    public async Task GetById_ReturnsStoreWithAddressOrNotFound()
    {
        var found = await Client.GetAsync($"/api/StoreApi/{TestData.StoreId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, found.StatusCode);
        Assert.Equal(TestData.StoreAddressId, (await ReadAsync<StoreVm>(found)).Address!.Id);

        Assert.Equal(HttpStatusCode.NotFound, (await Client.GetAsync("/api/StoreApi/999", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Client.GetAsync("/api/StoreApi/-1", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Client.GetAsync("/api/StoreApi/abc", TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Post_CreatesStoreWithAddressAndReturns201WithLocation()
    {
        var response = await Client.PostAsJsonAsync("/api/StoreApi", NewStore(), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await ReadAsync<StoreVm>(response);
        Assert.Equal(TestData.FreeAddressId, created.Address!.Id);
        Assert.EndsWith($"/api/StoreApi/{created.Id}", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Post_WithId_Returns400()
    {
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PostAsJsonAsync("/api/StoreApi", NewStore(id: 1), TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(1, Factory.WithDb(db => db.StationaryStores.Count()));
    }

    [Fact]
    public async Task Post_UnknownAddress_Returns400AndSavesNothing()
    {
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PostAsJsonAsync("/api/StoreApi", NewStore(addressId: 999), TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(1, Factory.WithDb(db => db.StationaryStores.Count()));
    }

    [Fact]
    public async Task Write_AddressUsedByAnotherStore_Returns400WithMessageAndSavesNothing()
    {
        var post = await Client.PostAsJsonAsync("/api/StoreApi", NewStore(addressId: TestData.StoreAddressId), TestContext.Current.CancellationToken);
        var second = await ReadAsync<StoreVm>(await Client.PostAsJsonAsync("/api/StoreApi", NewStore(), TestContext.Current.CancellationToken));
        var put = await Client.PutAsJsonAsync("/api/StoreApi", NewStore(second.Id, "Drugi", TestData.StoreAddressId), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, post.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, put.StatusCode);
        Assert.Contains("przypisany do innego sklepu", await put.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal(2, Factory.WithDb(db => db.StationaryStores.Count()));
        Assert.Equal(TestData.FreeAddressId, Factory.WithDb(db => db.StationaryStores.Single(s => s.Id == second.Id).AddressId));
    }

    [Fact]
    public async Task Put_StoreWithOwnAddress_Returns200()
    {
        var response = await Client.PutAsJsonAsync("/api/StoreApi", NewStore(TestData.StoreId, "Ta sama lokalizacja", TestData.StoreAddressId), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Put_UpdatesStore_UnknownIs404_MissingIdIs400()
    {
        var updated = await Client.PutAsJsonAsync("/api/StoreApi", NewStore(TestData.StoreId, "Zmieniona nazwa", TestData.StoreAddressId), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        Assert.Equal("Zmieniona nazwa", Factory.WithDb(db => db.StationaryStores.Single().Name));

        Assert.Equal(HttpStatusCode.NotFound, (await Client.PutAsJsonAsync("/api/StoreApi", NewStore(999), TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PutAsJsonAsync("/api/StoreApi", NewStore(), TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Write_WithInvalidBody_Returns400()
    {
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PostAsJsonAsync("/api/StoreApi", NewStore(name: ""), TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PostAsync("/api/StoreApi", RawJson("{zly json"), TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PutAsJsonAsync("/api/StoreApi", NewStore(TestData.StoreId, addressId: 999), TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task EveryRequest_WithoutToken_Returns401()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await Anonymous.GetAsync("/api/StoreApi", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await Anonymous.PostAsJsonAsync("/api/StoreApi", NewStore(), TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await Anonymous.PutAsJsonAsync("/api/StoreApi", NewStore(1), TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Delete_IsNotAvailable()
    {
        Assert.Equal(HttpStatusCode.MethodNotAllowed, (await Client.DeleteAsync("/api/StoreApi/1", TestContext.Current.CancellationToken)).StatusCode);
    }
}
