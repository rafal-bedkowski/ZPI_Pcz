using System.Linq.Expressions;
using WebStore.Model.DataModels;
using WebStore.ViewModels.VM;

namespace WebStore.Services.Interfaces;

public interface IStoreService
{
    StoreVm AddOrUpdateStore(AddOrUpdateStoreVm model);
    StoreVm? GetStore(Expression<Func<StationaryStore, bool>> filter);
    IEnumerable<StoreVm> GetStores(Expression<Func<StationaryStore, bool>>? filter = null);
}
