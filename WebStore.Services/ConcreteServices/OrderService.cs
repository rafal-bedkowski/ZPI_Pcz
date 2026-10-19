using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebStore.DAL.EF;
using WebStore.Model.DataModels;
using WebStore.Services.Interfaces;
using WebStore.ViewModels.VM;

namespace WebStore.Services.ConcreteServices;

public class OrderService : BaseService, IOrderService
{
    public OrderService(ApplicationDbContext dbContext, IMapper mapper, ILogger logger)
        : base(dbContext, mapper, logger)
    {
    }

    public OrderVm AddOrUpdateOrder(AddOrUpdateOrderVm model) => Execute(nameof(AddOrUpdateOrder), () =>
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(model.Products);

        Order? order = null;
        if (model.Id is not null and not 0)
        {
            order = WithItems(DbContext.Orders).FirstOrDefault(o => o.Id == model.Id)
                    ?? throw NotFound(nameof(Order), model.Id.Value);
        }

        if (!DbContext.Customers.Any(c => c.Id == model.CustomerId))
        {
            throw NotFound(nameof(Customer), model.CustomerId);
        }

        var productIds = model.Products.Select(p => p.ProductId).Distinct().ToList();
        var products = DbContext.Products.Where(p => productIds.Contains(p.Id)).ToDictionary(p => p.Id);
        var missing = productIds.FirstOrDefault(id => !products.ContainsKey(id));
        if (missing != 0)
        {
            throw NotFound(nameof(Product), missing);
        }

        if (order is null)
        {
            order = new Order();
            DbContext.Orders.Add(order);
        }

        order.CustomerId = model.CustomerId;
        order.OrderDate = model.OrderDate;
        order.DeliveryDate = model.DeliveryDate;
        order.TrackingNumber = model.TrackingNumber;
        SyncItems(order, model, products);

        DbContext.SaveChanges();
        return Mapper.Map<OrderVm>(order);
    });

    public OrderVm? GetOrder(Expression<Func<Order, bool>> filter) => Execute(nameof(GetOrder), () =>
    {
        ArgumentNullException.ThrowIfNull(filter);
        return Mapper.Map<OrderVm?>(WithItems(DbContext.Orders).FirstOrDefault(filter));
    });

    public IEnumerable<OrderVm> GetOrders(Expression<Func<Order, bool>>? filter = null) =>
        Execute(nameof(GetOrders), () =>
        {
            var query = WithItems(DbContext.Orders);
            if (filter is not null)
            {
                query = query.Where(filter);
            }

            return Mapper.Map<List<OrderVm>>(query.ToList());
        });

    private static IQueryable<Order> WithItems(IQueryable<Order> orders) =>
        orders.Include(o => o.OrderProducts).ThenInclude(op => op.Product).Include(o => o.Invoice);

    // Pozycja już obecna w zamówieniu zachowuje zamrożoną cenę; nowa dostaje aktualną cenę produktu.
    private static void SyncItems(Order order, AddOrUpdateOrderVm model, Dictionary<int, Product> products)
    {
        var wanted = model.Products
            .GroupBy(p => p.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Quantity));

        foreach (var obsolete in order.OrderProducts.Where(op => !wanted.ContainsKey(op.ProductId)).ToList())
        {
            order.OrderProducts.Remove(obsolete);
        }

        foreach (var (productId, quantity) in wanted)
        {
            var item = order.OrderProducts.FirstOrDefault(op => op.ProductId == productId);
            if (item is null)
            {
                order.OrderProducts.Add(new OrderProduct
                {
                    ProductId = productId,
                    Product = products[productId],
                    Quantity = quantity,
                    UnitPrice = products[productId].Price
                });
            }
            else
            {
                item.Quantity = quantity;
            }
        }
    }
}
