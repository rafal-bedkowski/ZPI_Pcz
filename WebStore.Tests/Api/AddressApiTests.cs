using System.Net;
using System.Net.Http.Json;
using WebStore.ViewModels.VM;

namespace WebStore.Tests.Api;

public class AddressApiTests : ApiTestBase
{
    private static AddOrUpdateAddressVm NewAddress(int? id = null, string street = "Armii Krajowej") => new()
    {
        Id = id, Street = street, BuildingNumber = "21", City = "Częstochowa", PostalCode = "42-200", Country = "Polska"
    };

    [Fact]
    public async Task GetAll_ReturnsEveryAddress()
    {
        var response = await Client.GetAsync("/api/AddressApi", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(Factory.WithDb(db => db.Addresses.Count()), (await ReadAsync<List<AddressVm>>(response)).Count);
    }

    [Fact]
    public async Task GetById_ReturnsAddressOrNotFound()
    {
        var found = await Client.GetAsync("/api/AddressApi/1", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, found.StatusCode);
        Assert.Equal(1, (await ReadAsync<AddressVm>(found)).Id);

        Assert.Equal(HttpStatusCode.NotFound, (await Client.GetAsync("/api/AddressApi/999", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Client.GetAsync("/api/AddressApi/0", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Client.GetAsync("/api/AddressApi/abc", TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Post_CreatesAddressAndReturns201WithLocation()
    {
        var response = await Client.PostAsJsonAsync("/api/AddressApi", NewAddress(), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await ReadAsync<AddressVm>(response);
        Assert.Equal("Armii Krajowej", created.Street);
        Assert.EndsWith($"/api/AddressApi/{created.Id}", response.Headers.Location!.ToString());
        Assert.Equal(5, Factory.WithDb(db => db.Addresses.Count()));
    }

    [Fact]
    public async Task Post_WithId_Returns400()
    {
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PostAsJsonAsync("/api/AddressApi", NewAddress(id: 1), TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(4, Factory.WithDb(db => db.Addresses.Count()));
    }

    [Fact]
    public async Task Put_UpdatesAddress_UnknownIs404_MissingIdIs400()
    {
        var updated = await Client.PutAsJsonAsync("/api/AddressApi", NewAddress(1, "Nowa"), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        Assert.Equal("Nowa", Factory.WithDb(db => db.Addresses.Single(a => a.Id == 1).Street));
        Assert.Equal(4, Factory.WithDb(db => db.Addresses.Count()));

        Assert.Equal(HttpStatusCode.NotFound, (await Client.PutAsJsonAsync("/api/AddressApi", NewAddress(999), TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PutAsJsonAsync("/api/AddressApi", NewAddress(), TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Write_WithInvalidBody_Returns400AndSavesNothing()
    {
        var noStreet = NewAddress();
        noStreet.Street = "";

        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PostAsJsonAsync("/api/AddressApi", noStreet, TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PutAsync("/api/AddressApi", RawJson("{zly json"), TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(4, Factory.WithDb(db => db.Addresses.Count()));
    }

    [Fact]
    public async Task EveryRequest_WithoutToken_Returns401()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await Anonymous.GetAsync("/api/AddressApi", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await Anonymous.GetAsync("/api/AddressApi/1", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await Anonymous.PostAsJsonAsync("/api/AddressApi", NewAddress(), TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await Anonymous.PutAsJsonAsync("/api/AddressApi", NewAddress(1), TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Delete_IsNotAvailable()
    {
        Assert.Equal(HttpStatusCode.MethodNotAllowed, (await Client.DeleteAsync("/api/AddressApi/1", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(4, Factory.WithDb(db => db.Addresses.Count()));
    }
}
