using Microsoft.EntityFrameworkCore;
using WebStore.DAL.EF;
using WebStore.Services.Interfaces;
using WebStore.Tests.Infrastructure;
using WebStore.ViewModels.VM;

namespace WebStore.Tests.UnitTests;

public class AddressServiceUnitTests : BaseUnitTests
{
    private readonly IAddressService _addressService;

    public AddressServiceUnitTests(ApplicationDbContext dbContext, IAddressService addressService) : base(dbContext)
    {
        _addressService = addressService;
    }

    private static AddOrUpdateAddressVm NewAddressVm(int? id, string city) => new()
    {
        Id = id, Street = "Długa", BuildingNumber = "5", City = city, PostalCode = "30-001", Country = "Polska"
    };

    [Fact]
    public void GetAddressTest()
    {
        var address = _addressService.GetAddress(a => a.Street == "Sklepowa");

        Assert.NotNull(address);
        Assert.Equal("Sklepowa", address.Street);
    }

    [Fact]
    public void GetMultipleAddressesTest()
    {
        var addresses = _addressService.GetAddresses(a => a.Id >= 1 && a.Id <= 2).ToList();

        Assert.Equal(2, addresses.Count);
    }

    [Fact]
    public void GetAllAddressesTest()
    {
        var addresses = _addressService.GetAddresses().ToList();

        Assert.Equal(DbContext.Addresses.Count(), addresses.Count);
    }

    [Fact]
    public void AddNewAddressTest()
    {
        var before = DbContext.Addresses.Count();

        var address = _addressService.AddOrUpdateAddress(NewAddressVm(null, "Kraków"));

        Assert.Equal("Długa", address.Street);
        Assert.Equal("Kraków", address.City);
        Assert.Equal(before + 1, DbContext.Addresses.Count());

        DbContext.ChangeTracker.Clear();
        var stored = DbContext.Addresses.AsNoTracking().Single(a => a.Id == address.Id);
        Assert.Equal("Długa", stored.Street);
        Assert.Equal("5", stored.BuildingNumber);
        Assert.Equal("Kraków", stored.City);
        Assert.Equal("30-001", stored.PostalCode);
        Assert.Equal("Polska", stored.Country);
    }

    [Fact]
    public void UpdateAddressTest()
    {
        var before = DbContext.Addresses.Count();

        var address = _addressService.AddOrUpdateAddress(NewAddressVm(TestData.FreeAddressId, "Gdańsk"));

        Assert.Equal(TestData.FreeAddressId, address.Id);
        Assert.Equal("Gdańsk", address.City);
        Assert.Equal(before, DbContext.Addresses.Count());

        DbContext.ChangeTracker.Clear();
        Assert.Equal(before, DbContext.Addresses.AsNoTracking().Count());
        var stored = DbContext.Addresses.AsNoTracking().Single(a => a.Id == TestData.FreeAddressId);
        Assert.Equal("Gdańsk", stored.City);
        Assert.Equal("Długa", stored.Street);
        Assert.Equal("5", stored.BuildingNumber);
    }
}
