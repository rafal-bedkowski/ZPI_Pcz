using WebStore.DAL.EF;
using WebStore.Services.ConcreteServices;
using WebStore.Services.Interfaces;
using WebStore.Tests.Infrastructure;
using WebStore.ViewModels.VM;

namespace WebStore.Tests;

public class StoreServiceTests
{
    private readonly IStoreService _service;
    private readonly ApplicationDbContext _db;
    private readonly TestLogger _logger;

    public StoreServiceTests(IStoreService service, ApplicationDbContext db, TestLogger logger)
    {
        _service = service;
        _db = db;
        _logger = logger;
    }

    [Fact]
    public void US_01_13_Structure_FollowsConventions()
    {
        Assert.True(typeof(IStoreService).IsPublic);
        Assert.True(typeof(StoreService).IsSubclassOf(typeof(BaseService)));
        Assert.Contains(typeof(IStoreService), typeof(StoreService).GetInterfaces());
        Assert.EndsWith("Vm", typeof(StoreVm).Name);
    }

    [Fact]
    public void US_01_13_AddOrUpdateStore_NewStore_IsPersistedWithAddress()
    {
        var result = _service.AddOrUpdateStore(new AddOrUpdateStoreVm { Name = "Sklep Północny", AddressId = TestData.FreeAddressId });

        Assert.True(result.Id > 0);
        Assert.Equal("Sklep Północny", result.Name);
        Assert.Equal(TestData.FreeAddressId, result.Address!.Id);
        Assert.Equal(TestData.FreeAddressId, _db.StationaryStores.Single(s => s.Id == result.Id).AddressId);
    }

    [Fact]
    public void US_01_13_AddOrUpdateStore_ExistingId_Updates()
    {
        var result = _service.AddOrUpdateStore(new AddOrUpdateStoreVm
        {
            Id = TestData.StoreId, Name = "Nowa nazwa", AddressId = TestData.StoreAddressId
        });

        Assert.Equal("Nowa nazwa", result.Name);
        Assert.Single(_db.StationaryStores);
    }

    [Fact]
    public void US_01_13_AddOrUpdateStore_UnknownId_ThrowsNotFound()
    {
        Assert.Throws<KeyNotFoundException>(() => _service.AddOrUpdateStore(
            new AddOrUpdateStoreVm { Id = 999, Name = "X", AddressId = TestData.FreeAddressId }));
        Assert.True(_logger.HasError<KeyNotFoundException>());
    }

    [Fact]
    public void US_01_13_GetStore_AndGetStores_FilterCorrectly()
    {
        Assert.Equal("Sklep Centralny", _service.GetStore(s => s.Id == TestData.StoreId)!.Name);
        Assert.Null(_service.GetStore(s => s.Name == "Brak"));
        Assert.Single(_service.GetStores(s => s.Name == "Sklep Centralny"));
        Assert.Equal(_db.StationaryStores.Count(), _service.GetStores().Count());
    }

    [Fact]
    public void US_01_13_NullArguments_ThrowArgumentNullAndAreLogged()
    {
        Assert.Throws<ArgumentNullException>(() => _service.AddOrUpdateStore(null!));
        Assert.Throws<ArgumentNullException>(() => _service.GetStore(null!));

        Assert.Equal(2, _logger.Entries.Count(e => e.Exception is ArgumentNullException));
    }
}
