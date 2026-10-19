using System.Linq.Expressions;
using WebStore.Model.DataModels;
using WebStore.ViewModels.VM;

namespace WebStore.Services.Interfaces;

public interface IOrderService
{
    OrderVm AddOrUpdateOrder(AddOrUpdateOrderVm model);
    OrderVm? GetOrder(Expression<Func<Order, bool>> filter);
    IEnumerable<OrderVm> GetOrders(Expression<Func<Order, bool>>? filter = null);
}
