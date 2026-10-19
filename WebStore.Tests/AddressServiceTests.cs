using WebStore.DAL.EF;
using WebStore.Services.ConcreteServices;
using WebStore.Services.Interfaces;
using WebStore.Tests.Infrastructure;
using WebStore.ViewModels.VM;

namespace WebStore.Tests;

public class AddressServiceTests
{
    private readonly IAddressService _service;
    private readonly ApplicationDbContext _db;
    private readonly TestLogger _logger;

    public AddressServiceTests(IAddressService service, ApplicationDbContext db, TestLogger logger)
    {
        _service = service;
        _db = db;
        _logger = logger;
    }

    private static AddOrUpdateAddressVm NewAddress(int? id = null, string city = "Kraków") => new()
    {
        Id = id, Street = "Długa", BuildingNumber = "5", ApartmentNumber = "12", City = city,
        PostalCode = "30-001", Country = "Polska"
    };

    [Fact]
    public void US_01_14_Structure_FollowsConventions()
    {
        Assert.True(typeof(IAddressService).IsPublic);
        Assert.True(typeof(AddressService).IsSubclassOf(typeof(BaseService)));
        Assert.Contains(typeof(IAddressService), typeof(AddressService).GetInterfaces());
        Assert.EndsWith("Vm", typeof(AddressVm).Name);
    }

    [Fact]
    public void US_01_14_AddOrUpdateAddress_NewAddress_IsPersistedAndReturned()
    {
        var before = _db.Addresses.Count();

        var result = _service.AddOrUpdateAddress(NewAddress());

        Assert.Equal(before + 1, _db.Addresses.Count());
        Assert.True(result.Id > 0);
        Assert.Equal("Długa", result.Street);
        Assert.Equal("Kraków", result.City);
    }

    [Fact]
    public void US_01_14_AddOrUpdateAddress_ExistingId_UpdatesWithoutNewRecord()
    {
        var before = _db.Addresses.Count();

        var result = _service.AddOrUpdateAddress(NewAddress(TestData.FreeAddressId, "Gdańsk"));

        Assert.Equal(before, _db.Addresses.Count());
        Assert.Equal("Gdańsk", result.City);
        Assert.Equal("Gdańsk", _db.Addresses.Single(a => a.Id == TestData.FreeAddressId).City);
    }

    [Fact]
    public void US_01_14_AddOrUpdateAddress_UnknownId_ThrowsNotFound()
    {
        Assert.Throws<KeyNotFoundException>(() => _service.AddOrUpdateAddress(NewAddress(999)));
        Assert.True(_logger.HasError<KeyNotFoundException>());
    }

    [Fact]
    public void US_01_14_GetAddress_AndGetAddresses_FilterCorrectly()
    {
        Assert.Equal("Sklepowa", _service.GetAddress(a => a.Id == TestData.StoreAddressId)!.Street);
        Assert.Null(_service.GetAddress(a => a.Id == 999));
        Assert.Single(_service.GetAddresses(a => a.Street == "Wolna"));
        Assert.Equal(_db.Addresses.Count(), _service.GetAddresses().Count());
    }

    [Fact]
    public void US_01_14_NullArguments_ThrowArgumentNullAndAreLogged()
    {
        Assert.Throws<ArgumentNullException>(() => _service.AddOrUpdateAddress(null!));
        Assert.Throws<ArgumentNullException>(() => _service.GetAddress(null!));

        Assert.Equal(2, _logger.Entries.Count(e => e.Exception is ArgumentNullException));
    }
}
