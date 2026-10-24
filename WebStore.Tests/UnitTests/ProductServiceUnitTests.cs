using Microsoft.EntityFrameworkCore;
using WebStore.DAL.EF;
using WebStore.Model.DataModels;
using WebStore.Services.Interfaces;
using WebStore.Tests.Infrastructure;
using WebStore.ViewModels.VM;

namespace WebStore.Tests.UnitTests;

public class ProductServiceUnitTests : BaseUnitTests
{
    private readonly IProductService _productService;

    public ProductServiceUnitTests(ApplicationDbContext dbContext, IProductService productService) : base(dbContext)
    {
        _productService = productService;
    }

    [Fact]
    public void GetProductTest()
    {
        var product = _productService.GetProduct(p => p.Name == "Monitor Dell 32");

        Assert.NotNull(product);
        Assert.Equal("Monitor Dell 32", product.Name);

        var second = _productService.GetProduct(p => p.Name == "Mysz Logitech");

        Assert.NotNull(second);
        Assert.Equal(TestData.MouseId, second.Id);
        Assert.Null(_productService.GetProduct(p => p.Name == "Nie istnieje"));
    }

    [Fact]
    public void GetMultipleProductsTest()
    {
        var outside = new Product
        {
            Name = "Klawiatura", Description = "Klawiatura mechaniczna", Price = 300m, Weight = 1f,
            CategoryId = TestData.CategoryId, SupplierId = TestData.SupplierId
        };
        DbContext.Products.Add(outside);
        DbContext.SaveChanges();

        var products = _productService.GetProducts(p => p.Id >= 1 && p.Id <= 2).ToList();

        Assert.Equal(2, products.Count);
        Assert.DoesNotContain(products, p => p.Id == outside.Id);
    }

    [Fact]
    public void GetAllProductsTest()
    {
        var products = _productService.GetProducts().ToList();

        Assert.Equal(DbContext.Products.Count(), products.Count);
    }

    [Fact]
    public void AddNewProductTest()
    {
        var before = DbContext.Products.Count();

        var product = _productService.AddOrUpdateProduct(new AddOrUpdateProductVm
        {
            Name = "MacBook Pro", Description = "Laptop", Price = 9000m, Weight = 1.6f,
            CategoryId = TestData.CategoryId, SupplierId = TestData.SupplierId
        });

        Assert.Equal("MacBook Pro", product.Name);
        Assert.Equal(before + 1, DbContext.Products.Count());

        DbContext.ChangeTracker.Clear();
        var stored = DbContext.Products.AsNoTracking().Single(p => p.Id == product.Id);
        Assert.Equal("MacBook Pro", stored.Name);
        Assert.Equal("Laptop", stored.Description);
        Assert.Equal(9000m, stored.Price);
        Assert.Equal(1.6f, stored.Weight);
        Assert.Equal(TestData.CategoryId, stored.CategoryId);
        Assert.Equal(TestData.SupplierId, stored.SupplierId);
    }

    [Fact]
    public void UpdateProductTest()
    {
        var before = DbContext.Products.Count();

        var product = _productService.AddOrUpdateProduct(new AddOrUpdateProductVm
        {
            Id = TestData.MonitorId, Name = "Monitor Dell 32", Description = "Monitor 32 cale 4K", Price = 2000m, Weight = 21f,
            CategoryId = TestData.CategoryId, SupplierId = TestData.SupplierId
        });

        Assert.Equal("Monitor Dell 32", product.Name);
        Assert.Equal(2000m, product.Price);
        Assert.Equal(before, DbContext.Products.Count());

        DbContext.ChangeTracker.Clear();
        Assert.Equal(before, DbContext.Products.AsNoTracking().Count());
        var stored = DbContext.Products.AsNoTracking().Single(p => p.Id == TestData.MonitorId);
        Assert.Equal("Monitor Dell 32", stored.Name);
        Assert.Equal("Monitor 32 cale 4K", stored.Description);
        Assert.Equal(2000m, stored.Price);
        Assert.Equal(21f, stored.Weight);
    }
}
