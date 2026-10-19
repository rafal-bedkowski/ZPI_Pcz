using System.Linq.Expressions;
using WebStore.Model.DataModels;
using WebStore.ViewModels.VM;

namespace WebStore.Services.Interfaces;

public interface IAddressService
{
    AddressVm AddOrUpdateAddress(AddOrUpdateAddressVm model);
    AddressVm? GetAddress(Expression<Func<Address, bool>> filter);
    IEnumerable<AddressVm> GetAddresses(Expression<Func<Address, bool>>? filter = null);
}
