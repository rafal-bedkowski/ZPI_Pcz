using Microsoft.EntityFrameworkCore;
using WebStore.DAL.EF;
using WebStore.Services.Interfaces;
using WebStore.Tests.Infrastructure;
using WebStore.ViewModels.VM;

namespace WebStore.Tests;

public class ProductVisibilityServiceTests
{
    private readonly IProductService _products;
    private readonly IOrderService _orders;
    private readonly ApplicationDbContext _db;
    private readonly TestLogger _logger;

    public ProductVisibilityServiceTests(IProductService products, IOrderService orders, ApplicationDbContext db, TestLogger logger)
    {
        _products = products;
        _orders = orders;
        _db = db;
        _logger = logger;
    }

    private static AddOrUpdateOrderVm NewOrder(int? id, params int[] productIds) => new()
    {
        Id = id, CustomerId = TestData.CustomerId, OrderDate = new DateTime(2026, 1, 10), DeliveryDate = new DateTime(2026, 1, 15),
        TrackingNumber = 1,
        Products = productIds.Select(p => new AddOrUpdateOrderProductVm { ProductId = p, Quantity = 1 }).ToList()
    };

    [Fact]
    public void HideProduct_KeepsRecordAndSetsFlag()
    {
        _products.HideProduct(TestData.MonitorId);

        Assert.True(_db.Products.Single(p => p.Id == TestData.MonitorId).IsHidden);
        Assert.Equal(2, _db.Products.Count());
    }

    [Fact]
    public void HiddenProduct_IsNotReturnedByGetProductAndGetProducts()
    {
        _products.HideProduct(TestData.MonitorId);

        Assert.Null(_products.GetProduct(p => p.Id == TestData.MonitorId));
        Assert.DoesNotContain(_products.GetProducts(), p => p.Id == TestData.MonitorId);
        Assert.DoesNotContain(_products.GetProducts(p => p.Price > 0), p => p.Id == TestData.MonitorId);
    }

    [Fact]
    public void HideProduct_UnknownOrAlreadyHidden_ThrowsNotFoundAndLogs()
    {
        Assert.Throws<KeyNotFoundException>(() => _products.HideProduct(999));
        _products.HideProduct(TestData.MouseId);
        Assert.Throws<KeyNotFoundException>(() => _products.HideProduct(TestData.MouseId));

        Assert.True(_logger.HasError<KeyNotFoundException>());
        Assert.Equal(1, _db.Products.Count(p => p.IsHidden));
    }

    [Fact]
    public void HiddenProduct_CannotBeUpdated()
    {
        _products.HideProduct(TestData.MonitorId);

        Assert.Throws<KeyNotFoundException>(() => _products.AddOrUpdateProduct(new AddOrUpdateProductVm
        {
            Id = TestData.MonitorId, Name = "Monitor", Description = "opis", Price = 1m,
            CategoryId = TestData.CategoryId, SupplierId = TestData.SupplierId
        }));
    }

    [Fact]
    public void UpdatingProduct_DoesNotChangeHiddenFlag()
    {
        _products.AddOrUpdateProduct(new AddOrUpdateProductVm
        {
            Id = TestData.MonitorId, Name = "Monitor", Description = "opis", Price = 1m,
            CategoryId = TestData.CategoryId, SupplierId = TestData.SupplierId
        });

        Assert.False(_db.Products.Single(p => p.Id == TestData.MonitorId).IsHidden);
    }

    [Fact]
    public void Order_WithHiddenProduct_IsRejectedAndNothingIsSaved()
    {
        _products.HideProduct(TestData.MonitorId);

        Assert.Throws<System.ComponentModel.DataAnnotations.ValidationException>(
            () => _orders.AddOrUpdateOrder(NewOrder(null, TestData.MonitorId, TestData.MouseId)));

        Assert.Empty(_db.Orders);
        Assert.Empty(_db.OrderProducts);
    }

    [Fact]
    public void ExistingOrder_WithLaterHiddenProduct_KeepsItemsFrozenPriceAndTotal()
    {
        var order = _orders.AddOrUpdateOrder(NewOrder(null, TestData.MonitorId, TestData.MouseId));
        _products.HideProduct(TestData.MonitorId);

        var fetched = _orders.GetOrder(o => o.Id == order.Id)!;

        Assert.Equal(2, fetched.Products.Count);
        Assert.Equal(1000m, fetched.Products.Single(p => p.ProductId == TestData.MonitorId).UnitPrice);
        Assert.Equal(order.TotalAmount, fetched.TotalAmount);
        Assert.Equal(1500m, fetched.TotalAmount);
        Assert.Equal(1, _db.Orders.Include(o => o.OrderProducts).Count());
    }
}
