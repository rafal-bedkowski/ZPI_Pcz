using Microsoft.EntityFrameworkCore;
using WebStore.DAL.EF;
using WebStore.Model.DataModels;
using WebStore.Services.Interfaces;
using WebStore.Tests.Infrastructure;
using WebStore.ViewModels.VM;

namespace WebStore.Tests.UnitTests;

public class OrderServiceUnitTests : BaseUnitTests
{
    private readonly IOrderService _orderService;

    public OrderServiceUnitTests(ApplicationDbContext dbContext, IOrderService orderService) : base(dbContext)
    {
        _orderService = orderService;
    }

    // Zamówienie zapisywane bezpośrednio w bazie, żeby testy odczytu nie zależały od zapisu przez usługę.
    private Order SaveOrder(long trackingNumber)
    {
        var order = new Order
        {
            CustomerId = TestData.CustomerId, OrderDate = new DateTime(2025, 3, 1),
            DeliveryDate = new DateTime(2025, 3, 5), TrackingNumber = trackingNumber
        };
        order.OrderProducts.Add(new OrderProduct { ProductId = TestData.MonitorId, Quantity = 1, UnitPrice = 1000m });
        DbContext.Orders.Add(order);
        DbContext.SaveChanges();
        return order;
    }

    private static AddOrUpdateOrderVm NewOrderVm(int? id, long trackingNumber) => new()
    {
        Id = id, CustomerId = TestData.CustomerId, OrderDate = new DateTime(2025, 3, 1),
        DeliveryDate = new DateTime(2025, 3, 5), TrackingNumber = trackingNumber,
        Products = { new AddOrUpdateOrderProductVm { ProductId = TestData.MouseId, Quantity = 2 } }
    };

    [Fact]
    public void GetOrderTest()
    {
        SaveOrder(111);
        var saved = SaveOrder(222);

        var order = _orderService.GetOrder(o => o.TrackingNumber == 222);

        Assert.NotNull(order);
        Assert.Equal(saved.Id, order.Id);
        Assert.Equal(222, order.TrackingNumber);
        Assert.Null(_orderService.GetOrder(o => o.TrackingNumber == 999));
    }

    [Fact]
    public void GetMultipleOrdersTest()
    {
        var first = SaveOrder(111);
        var second = SaveOrder(222);
        var outside = SaveOrder(333);

        var orders = _orderService.GetOrders(o => o.Id >= first.Id && o.Id <= second.Id).ToList();

        Assert.Equal(2, orders.Count);
        Assert.DoesNotContain(orders, o => o.Id == outside.Id);
    }

    [Fact]
    public void GetAllOrdersTest()
    {
        SaveOrder(111);
        SaveOrder(222);

        var orders = _orderService.GetOrders().ToList();

        Assert.Equal(DbContext.Orders.Count(), orders.Count);
    }

    [Fact]
    public void AddNewOrderTest()
    {
        var before = DbContext.Orders.Count();

        var order = _orderService.AddOrUpdateOrder(NewOrderVm(null, 333));

        Assert.Equal(333, order.TrackingNumber);
        Assert.Equal(TestData.CustomerId, order.CustomerId);
        Assert.Equal(before + 1, DbContext.Orders.Count());

        DbContext.ChangeTracker.Clear();
        var stored = DbContext.Orders.AsNoTracking().Include(o => o.OrderProducts)
            .Single(o => o.Id == order.Id);
        Assert.Equal(333, stored.TrackingNumber);
        Assert.Equal(TestData.CustomerId, stored.CustomerId);
        Assert.Equal(new DateTime(2025, 3, 1), stored.OrderDate);
        Assert.Equal(new DateTime(2025, 3, 5), stored.DeliveryDate);
        var item = Assert.Single(stored.OrderProducts);
        Assert.Equal(TestData.MouseId, item.ProductId);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(500m, item.UnitPrice);
        Assert.Equal(1000m, stored.TotalAmount);
    }

    [Fact]
    public void UpdateOrderTest()
    {
        var saved = SaveOrder(111);
        var before = DbContext.Orders.Count();

        var order = _orderService.AddOrUpdateOrder(NewOrderVm(saved.Id, 444));

        Assert.Equal(saved.Id, order.Id);
        Assert.Equal(444, order.TrackingNumber);
        Assert.Equal(before, DbContext.Orders.Count());

        DbContext.ChangeTracker.Clear();
        Assert.Equal(before, DbContext.Orders.AsNoTracking().Count());
        var stored = DbContext.Orders.AsNoTracking().Include(o => o.OrderProducts)
            .Single(o => o.Id == saved.Id);
        Assert.Equal(444, stored.TrackingNumber);
        var item = Assert.Single(stored.OrderProducts);
        Assert.Equal(TestData.MouseId, item.ProductId);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(500m, item.UnitPrice);
        Assert.Equal(1000m, stored.TotalAmount);
        Assert.Equal(1, DbContext.OrderProducts.AsNoTracking().Count());
    }
}
