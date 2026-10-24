using Microsoft.EntityFrameworkCore;
using WebStore.DAL.EF;
using WebStore.Model.DataModels;
using WebStore.Services.Interfaces;
using WebStore.Tests.Infrastructure;
using WebStore.ViewModels.VM;

namespace WebStore.Tests.UnitTests;

public class StoreServiceUnitTests : BaseUnitTests
{
    private readonly IStoreService _storeService;

    public StoreServiceUnitTests(ApplicationDbContext dbContext, IStoreService storeService) : base(dbContext)
    {
        _storeService = storeService;
    }

    // Sklep z własnym adresem zapisany bezpośrednio w bazie, żeby nie zajmować adresów używanych w innych testach.
    private StationaryStore SaveStore(string name, string street)
    {
        var address = new Address
        {
            Street = street, BuildingNumber = "1", City = "Kraków", PostalCode = "30-001", Country = "Polska"
        };
        var store = new StationaryStore { Name = name, Address = address };
        DbContext.StationaryStores.Add(store);
        DbContext.SaveChanges();
        return store;
    }

    [Fact]
    public void GetStoreTest()
    {
        var other = SaveStore("Sklep Zachodni", "Zachodnia");

        var store = _storeService.GetStore(s => s.Name == "Sklep Centralny");

        Assert.NotNull(store);
        Assert.Equal(TestData.StoreId, store.Id);
        Assert.Equal("Sklep Centralny", store.Name);

        var second = _storeService.GetStore(s => s.Name == "Sklep Zachodni");

        Assert.NotNull(second);
        Assert.Equal(other.Id, second.Id);
        Assert.Null(_storeService.GetStore(s => s.Name == "Nie istnieje"));
    }

    [Fact]
    public void GetMultipleStoresTest()
    {
        DbContext.StationaryStores.Add(new StationaryStore { Name = "Sklep Północny", AddressId = TestData.FreeAddressId });
        DbContext.SaveChanges();
        var outside = SaveStore("Sklep Zachodni", "Zachodnia");

        var stores = _storeService.GetStores(s => s.Id >= 1 && s.Id <= 2).ToList();

        Assert.Equal(2, stores.Count);
        Assert.DoesNotContain(stores, s => s.Id == outside.Id);
    }

    [Fact]
    public void GetAllStoresTest()
    {
        var stores = _storeService.GetStores().ToList();

        Assert.Equal(DbContext.StationaryStores.Count(), stores.Count);
    }

    [Fact]
    public void AddNewStoreTest()
    {
        var before = DbContext.StationaryStores.Count();

        var store = _storeService.AddOrUpdateStore(new AddOrUpdateStoreVm { Name = "Sklep Północny", AddressId = TestData.FreeAddressId });

        Assert.Equal("Sklep Północny", store.Name);
        Assert.Equal(before + 1, DbContext.StationaryStores.Count());

        DbContext.ChangeTracker.Clear();
        var stored = DbContext.StationaryStores.AsNoTracking().Single(s => s.Id == store.Id);
        Assert.Equal("Sklep Północny", stored.Name);
        Assert.Equal(TestData.FreeAddressId, stored.AddressId);
    }

    [Fact]
    public void UpdateStoreTest()
    {
        var before = DbContext.StationaryStores.Count();

        var store = _storeService.AddOrUpdateStore(new AddOrUpdateStoreVm
        {
            Id = TestData.StoreId, Name = "Nowa nazwa", AddressId = TestData.StoreAddressId
        });

        Assert.Equal(TestData.StoreId, store.Id);
        Assert.Equal("Nowa nazwa", store.Name);
        Assert.Equal(before, DbContext.StationaryStores.Count());

        DbContext.ChangeTracker.Clear();
        Assert.Equal(before, DbContext.StationaryStores.AsNoTracking().Count());
        var stored = DbContext.StationaryStores.AsNoTracking().Single(s => s.Id == TestData.StoreId);
        Assert.Equal("Nowa nazwa", stored.Name);
        Assert.Equal(TestData.StoreAddressId, stored.AddressId);
    }
}
