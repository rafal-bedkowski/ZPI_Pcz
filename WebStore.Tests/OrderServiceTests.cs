using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using WebStore.DAL.EF;
using WebStore.Services.ConcreteServices;
using WebStore.Services.Interfaces;
using WebStore.Tests.Infrastructure;
using WebStore.ViewModels.VM;

namespace WebStore.Tests;

public class OrderServiceTests
{
    private readonly IOrderService _service;
    private readonly IProductService _products;
    private readonly ApplicationDbContext _db;
    private readonly TestLogger _logger;

    public OrderServiceTests(IOrderService service, IProductService products, ApplicationDbContext db, TestLogger logger)
    {
        _service = service;
        _products = products;
        _db = db;
        _logger = logger;
    }

    private static AddOrUpdateOrderVm NewOrder(int? id = null, int customerId = TestData.CustomerId, params (int Product, int Qty)[] items) => new()
    {
        Id = id, CustomerId = customerId, OrderDate = new DateTime(2025, 3, 1), DeliveryDate = new DateTime(2025, 3, 5),
        TrackingNumber = 123456789012L,
        Products = items.Select(i => new AddOrUpdateOrderProductVm { ProductId = i.Product, Quantity = i.Qty }).ToList()
    };

    [Fact]
    public void US_01_11_Structure_FollowsConventions()
    {
        Assert.True(typeof(IOrderService).IsPublic);
        Assert.True(typeof(OrderService).IsSubclassOf(typeof(BaseService)));
        Assert.Contains(typeof(IOrderService), typeof(OrderService).GetInterfaces());
        Assert.EndsWith("Vm", typeof(OrderVm).Name);
        Assert.Equal("WebStore.ViewModels.VM", typeof(OrderVm).Namespace);
    }

    [Fact]
    public void US_01_11_AddOrUpdateOrder_PersistsItemsWithFrozenCurrentPrice()
    {
        var result = _service.AddOrUpdateOrder(NewOrder(null, TestData.CustomerId, (TestData.MonitorId, 2), (TestData.MouseId, 1)));

        Assert.True(result.Id > 0);
        Assert.Equal(TestData.CustomerId, result.CustomerId);
        Assert.Equal(2, result.Products.Count);
        Assert.Equal(1000m, result.Products.Single(p => p.ProductId == TestData.MonitorId).UnitPrice);
        Assert.Equal(2500m, result.TotalAmount);
        Assert.Equal(2, _db.OrderProducts.Count(op => op.OrderId == result.Id));
    }

    [Fact]
    public void US_01_11_ProductPriceChange_DoesNotAffectExistingOrder()
    {
        var order = _service.AddOrUpdateOrder(NewOrder(null, TestData.CustomerId, (TestData.MouseId, 2)));
        _products.AddOrUpdateProduct(new AddOrUpdateProductVm
        {
            Id = TestData.MouseId, Name = "Mysz Logitech", Description = "x", Price = 1000m, Weight = 0.5f,
            CategoryId = TestData.CategoryId, SupplierId = TestData.SupplierId
        });

        var reloaded = _service.GetOrder(o => o.Id == order.Id)!;

        Assert.Equal(500m, reloaded.Products.Single().UnitPrice);
        Assert.Equal(1000m, reloaded.TotalAmount);
    }

    [Fact]
    public void US_01_11_AddOrUpdateOrder_ExistingOrder_KeepsFrozenPriceAndAddsNewItemsAtCurrentPrice()
    {
        var order = _service.AddOrUpdateOrder(NewOrder(null, TestData.CustomerId, (TestData.MouseId, 1)));
        _products.AddOrUpdateProduct(new AddOrUpdateProductVm
        {
            Id = TestData.MouseId, Name = "Mysz Logitech", Description = "x", Price = 900m, Weight = 0.5f,
            CategoryId = TestData.CategoryId, SupplierId = TestData.SupplierId
        });

        var updated = _service.AddOrUpdateOrder(NewOrder(order.Id, TestData.CustomerId, (TestData.MouseId, 3), (TestData.MonitorId, 1)));

        Assert.Equal(500m, updated.Products.Single(p => p.ProductId == TestData.MouseId).UnitPrice);
        Assert.Equal(1000m, updated.Products.Single(p => p.ProductId == TestData.MonitorId).UnitPrice);
        Assert.Equal(2500m, updated.TotalAmount);
        Assert.Single(_db.Orders);
    }

    [Fact]
    public void US_01_11_GetOrder_AndGetOrders_FilterCorrectly()
    {
        var first = _service.AddOrUpdateOrder(NewOrder(null, TestData.CustomerId, (TestData.MonitorId, 1)));
        _service.AddOrUpdateOrder(NewOrder(null, TestData.CustomerId, (TestData.MouseId, 1)));

        Assert.Equal(first.Id, _service.GetOrder(o => o.Id == first.Id)!.Id);
        Assert.Null(_service.GetOrder(o => o.Id == 999));
        Assert.Single(_service.GetOrders(o => o.Id == first.Id));
        Assert.Equal(_db.Orders.Count(), _service.GetOrders().Count());
    }

    [Fact]
    public void US_01_11_NullArguments_ThrowArgumentNullAndAreLogged()
    {
        Assert.Throws<ArgumentNullException>(() => _service.AddOrUpdateOrder(null!));
        Assert.Throws<ArgumentNullException>(() => _service.GetOrder(null!));

        Assert.Equal(2, _logger.Entries.Count(e => e.Exception is ArgumentNullException));
    }

    [Fact]
    public void US_01_11_UnknownCustomerOrProduct_FailsAndSavesNothing()
    {
        Assert.Throws<KeyNotFoundException>(() => _service.AddOrUpdateOrder(NewOrder(null, 999, (TestData.MonitorId, 1))));

        Assert.Empty(_db.Orders);
        Assert.Empty(_db.OrderProducts);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void US_01_11_NonPositiveQuantity_IsRejectedWithValidationErrorAndNothingSaved(int quantity)
    {
        Assert.Throws<ValidationException>(() => _service.AddOrUpdateOrder(NewOrder(null, TestData.CustomerId, (TestData.MonitorId, quantity))));

        Assert.True(_logger.HasError<ValidationException>());
        Assert.Empty(_db.Orders);
        Assert.Empty(_db.OrderProducts);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public void US_01_11_InvalidOrUnknownProduct_IsRejectedWithValidationErrorAndNothingSaved(int productId)
    {
        Assert.Throws<ValidationException>(() => _service.AddOrUpdateOrder(NewOrder(null, TestData.CustomerId, (productId, 1))));

        Assert.Empty(_db.Orders);
        Assert.Empty(_db.OrderProducts);
        Assert.Empty(_db.ChangeTracker.Entries<Model.DataModels.Order>());
    }

    [Fact]
    public void US_01_11_EmptyItemList_IsRejectedWithValidationErrorAndNothingSaved()
    {
        Assert.Throws<ValidationException>(() => _service.AddOrUpdateOrder(NewOrder(null, TestData.CustomerId)));

        Assert.Empty(_db.Orders);
    }

    [Fact]
    public void US_01_11_AddOrUpdateOrder_UnknownId_ThrowsNotFound()
    {
        Assert.Throws<KeyNotFoundException>(() => _service.AddOrUpdateOrder(NewOrder(999, TestData.CustomerId, (TestData.MonitorId, 1))));

        Assert.True(_logger.HasError<KeyNotFoundException>());
    }

    [Fact]
    public void US_01_11_TotalAmount_FractionalPrices_IsExactAfterSaveAndLoad()
    {
        var monitor = _db.Products.Single(p => p.Id == TestData.MonitorId);
        var mouse = _db.Products.Single(p => p.Id == TestData.MouseId);
        monitor.Price = 19.99m;
        mouse.Price = 0.01m;
        _db.SaveChanges();

        var saved = _service.AddOrUpdateOrder(NewOrder(null, TestData.CustomerId, (TestData.MonitorId, 3), (TestData.MouseId, 7)));
        _db.ChangeTracker.Clear();
        var loaded = _service.GetOrder(o => o.Id == saved.Id)!;

        Assert.Equal(60.04m, saved.TotalAmount);
        Assert.Equal(60.04m, loaded.TotalAmount);
    }
}
