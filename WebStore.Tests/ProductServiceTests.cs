using Microsoft.Extensions.Logging;
using WebStore.DAL.EF;
using WebStore.Model.DataModels;
using WebStore.Services.Interfaces;
using WebStore.Tests.Infrastructure;
using WebStore.ViewModels.VM;

namespace WebStore.Tests;

public class ProductServiceTests
{
    private readonly IProductService _service;
    private readonly ApplicationDbContext _db;
    private readonly TestLogger _logger;

    public ProductServiceTests(IProductService service, ApplicationDbContext db, TestLogger logger)
    {
        _service = service;
        _db = db;
        _logger = logger;
    }

    private static AddOrUpdateProductVm NewProduct(int? id = null, string name = "Klawiatura", decimal price = 250m) => new()
    {
        Id = id, Name = name, Description = "opis", Price = price, Weight = 1.2f,
        CategoryId = TestData.CategoryId, SupplierId = TestData.SupplierId
    };

    [Fact]
    public void US_01_09_Interface_DeclaresExpectedMethods()
    {
        var type = typeof(IProductService);

        Assert.Equal(typeof(ProductVm), type.GetMethod("AddOrUpdateProduct")!.ReturnType);
        Assert.Equal(typeof(ProductVm), type.GetMethod("GetProduct")!.ReturnType);
        Assert.Equal(typeof(IEnumerable<ProductVm>), type.GetMethod("GetProducts")!.ReturnType);
        Assert.True(type.GetMethod("GetProducts")!.GetParameters().Single().HasDefaultValue);
    }

    [Fact]
    public void US_01_09_ViewModels_HaveExpectedMembers()
    {
        var addOrUpdate = typeof(AddOrUpdateProductVm).GetProperties().Select(p => p.Name);
        var product = typeof(ProductVm).GetProperties().Select(p => p.Name);

        foreach (var name in new[] { "Id", "Description", "ImageBytes", "Name", "Price", "Weight", "CategoryId", "SupplierId" })
        {
            Assert.Contains(name, addOrUpdate);
        }

        foreach (var name in new[] { "Description", "ImageBytes", "Name", "Price", "Weight", "Quantity" })
        {
            Assert.Contains(name, product);
        }

        Assert.Equal(typeof(int?), typeof(AddOrUpdateProductVm).GetProperty("Id")!.PropertyType);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    public void US_01_09_AddOrUpdateProduct_EmptyOrZeroId_AddsNewRecord(int? id)
    {
        var before = _db.Products.Count();

        var result = _service.AddOrUpdateProduct(NewProduct(id, "Klawiatura", 250m));

        Assert.Equal(before + 1, _db.Products.Count());
        Assert.Equal("Klawiatura", result.Name);
        Assert.Equal(250m, result.Price);
    }

    [Fact]
    public void US_01_09_AddOrUpdateProduct_ExistingId_ModifiesWithoutCreatingNew()
    {
        var before = _db.Products.Count();

        var result = _service.AddOrUpdateProduct(NewProduct(1, "Monitor Dell 32", 2000m));

        Assert.Equal(2000m, result.Price);
        Assert.Equal(before, _db.Products.Count());
        Assert.Equal(2000m, _db.Products.Single(p => p.Id == 1).Price);
    }

    [Fact]
    public void US_01_09_AddOrUpdateProduct_UnknownId_ThrowsNotFoundLogsAndAddsNothing()
    {
        var before = _db.Products.Count();

        Assert.Throws<KeyNotFoundException>(() => _service.AddOrUpdateProduct(NewProduct(999)));

        Assert.True(_logger.HasError<KeyNotFoundException>());
        Assert.Equal(before, _db.Products.Count());
    }

    [Fact]
    public void US_01_09_AddOrUpdateProduct_Null_ThrowsArgumentNullAndLogs()
    {
        Assert.Throws<ArgumentNullException>(() => _service.AddOrUpdateProduct(null!));

        Assert.True(_logger.HasError<ArgumentNullException>());
    }

    [Fact]
    public void US_01_09_GetProduct_NullFilter_ThrowsArgumentNullAndLogs()
    {
        Assert.Throws<ArgumentNullException>(() => _service.GetProduct(null!));

        Assert.True(_logger.HasError<ArgumentNullException>());
    }

    [Fact]
    public void US_01_09_GetProduct_ByName_ReturnsMatchingProductWithStockQuantity()
    {
        var result = _service.GetProduct(p => p.Name == "Monitor Dell 32");

        Assert.NotNull(result);
        Assert.Equal("Monitor Dell 32", result.Name);
        Assert.Equal(10, result.Quantity);
    }

    [Fact]
    public void US_01_09_GetProducts_FilterAndNoFilter()
    {
        var filtered = _service.GetProducts(p => p.Id >= 1 && p.Id <= 2).ToList();
        var all = _service.GetProducts().ToList();

        Assert.Equal(2, filtered.Count);
        Assert.Equal(_db.Products.Count(), all.Count);
    }

    [Fact]
    public void US_01_09_GetProduct_NoMatch_ReturnsNull()
    {
        Assert.Null(_service.GetProduct(p => p.Name == "Nie istnieje"));
    }

    [Fact]
    public void US_01_09_GetProducts_PriceFilters_ReturnOnlyMatchingProducts()
    {
        var expensive = _service.GetProducts(p => p.Price > 600).Single();
        var cheap = _service.GetProducts(p => p.Price <= 500).Single();

        Assert.Equal("Monitor Dell 32", expensive.Name);
        Assert.Equal("Mysz Logitech", cheap.Name);
    }
}
