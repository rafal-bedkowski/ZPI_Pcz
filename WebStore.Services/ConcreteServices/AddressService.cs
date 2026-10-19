using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using WebStore.DAL.EF;
using WebStore.Model.DataModels;
using WebStore.Services.Interfaces;
using WebStore.ViewModels.VM;

namespace WebStore.Services.ConcreteServices;

public class AddressService : BaseService, IAddressService
{
    public AddressService(ApplicationDbContext dbContext, IMapper mapper, ILogger logger)
        : base(dbContext, mapper, logger)
    {
    }

    public AddressVm AddOrUpdateAddress(AddOrUpdateAddressVm model) => Execute(nameof(AddOrUpdateAddress), () =>
    {
        ArgumentNullException.ThrowIfNull(model);

        Address address;
        if (model.Id is null or 0)
        {
            address = Mapper.Map<Address>(model);
            DbContext.Addresses.Add(address);
        }
        else
        {
            address = DbContext.Addresses.FirstOrDefault(a => a.Id == model.Id)
                      ?? throw NotFound(nameof(Address), model.Id.Value);
            Mapper.Map(model, address);
        }

        DbContext.SaveChanges();
        return Mapper.Map<AddressVm>(address);
    });

    public AddressVm? GetAddress(Expression<Func<Address, bool>> filter) => Execute(nameof(GetAddress), () =>
    {
        ArgumentNullException.ThrowIfNull(filter);
        return Mapper.Map<AddressVm?>(DbContext.Addresses.FirstOrDefault(filter));
    });

    public IEnumerable<AddressVm> GetAddresses(Expression<Func<Address, bool>>? filter = null) =>
        Execute(nameof(GetAddresses), () =>
        {
            IQueryable<Address> query = DbContext.Addresses;
            if (filter is not null)
            {
                query = query.Where(filter);
            }

            return Mapper.Map<List<AddressVm>>(query.ToList());
        });
}
