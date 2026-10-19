using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebStore.DAL.EF;
using WebStore.Model.DataModels;
using WebStore.Services.Interfaces;
using WebStore.ViewModels.VM;

namespace WebStore.Services.ConcreteServices;

public class StoreService : BaseService, IStoreService
{
    public StoreService(ApplicationDbContext dbContext, IMapper mapper, ILogger logger)
        : base(dbContext, mapper, logger)
    {
    }

    public StoreVm AddOrUpdateStore(AddOrUpdateStoreVm model) => Execute(nameof(AddOrUpdateStore), () =>
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!DbContext.Addresses.Any(a => a.Id == model.AddressId))
        {
            throw NotFound(nameof(Address), model.AddressId);
        }

        StationaryStore store;
        if (model.Id is null or 0)
        {
            store = Mapper.Map<StationaryStore>(model);
            DbContext.StationaryStores.Add(store);
        }
        else
        {
            store = DbContext.StationaryStores.FirstOrDefault(s => s.Id == model.Id)
                    ?? throw NotFound(nameof(StationaryStore), model.Id.Value);
            Mapper.Map(model, store);
        }

        DbContext.SaveChanges();
        return Mapper.Map<StoreVm>(DbContext.StationaryStores.Include(s => s.Address).First(s => s.Id == store.Id));
    });

    public StoreVm? GetStore(Expression<Func<StationaryStore, bool>> filter) => Execute(nameof(GetStore), () =>
    {
        ArgumentNullException.ThrowIfNull(filter);
        return Mapper.Map<StoreVm?>(DbContext.StationaryStores.Include(s => s.Address).FirstOrDefault(filter));
    });

    public IEnumerable<StoreVm> GetStores(Expression<Func<StationaryStore, bool>>? filter = null) =>
        Execute(nameof(GetStores), () =>
        {
            IQueryable<StationaryStore> query = DbContext.StationaryStores.Include(s => s.Address);
            if (filter is not null)
            {
                query = query.Where(filter);
            }

            return Mapper.Map<List<StoreVm>>(query.ToList());
        });
}
