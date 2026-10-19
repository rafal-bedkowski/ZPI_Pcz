using Microsoft.EntityFrameworkCore;
using WebStore.DAL.EF;
using WebStore.Model.DataModels;
using WebStore.Tests.Infrastructure;

namespace WebStore.Tests;

public class SeedDataTests
{
    private readonly ApplicationDbContext _db;

    public SeedDataTests(ApplicationDbContext db) => _db = db;

    [Fact]
    public void US_01_10_Seed_ContainsSupplierCategoryAndProducts()
    {
        var supplier = _db.Suppliers.Single(s => s.Id == 1);
        Assert.Equal("Adam", supplier.FirstName);
        Assert.Equal("Bednarski", supplier.LastName);
        Assert.Equal("supp1@eg.eg", supplier.Email);

        var category = _db.Categories.Single(c => c.Id == 1);
        Assert.Equal("Computers", category.Name);
        Assert.Equal("#computer", category.Tag);

        var monitor = _db.Products.Single(p => p.Id == 1);
        Assert.Equal("Monitor Dell 32", monitor.Name);
        Assert.Equal(1000m, monitor.Price);
        Assert.Equal(20f, monitor.Weight);

        var mouse = _db.Products.Single(p => p.Id == 2);
        Assert.Equal("Mysz Logitech", mouse.Name);
        Assert.Equal(500m, mouse.Price);
        Assert.Equal(0.5f, mouse.Weight);
    }

    [Fact]
    public void US_01_10_ForeignKeys_AreEnforcedByTestDatabase()
    {
        _db.Products.Add(new Product { Name = "X", Description = "X", CategoryId = 99, SupplierId = 1 });

        Assert.Throws<DbUpdateException>(() => _db.SaveChanges());
    }

    [Fact]
    public void US_01_10_Database_IsolatedPerTest_FirstWriter()
    {
        _db.Products.Add(new Product { Name = "Izolacja", Description = "d", CategoryId = 1, SupplierId = 1 });
        _db.SaveChanges();

        Assert.Equal(3, _db.Products.Count());
    }

    [Fact]
    public void US_01_10_Database_IsolatedPerTest_SecondWriter()
    {
        _db.Products.Add(new Product { Name = "Izolacja", Description = "d", CategoryId = 1, SupplierId = 1 });
        _db.SaveChanges();

        Assert.Equal(3, _db.Products.Count());
    }
}
