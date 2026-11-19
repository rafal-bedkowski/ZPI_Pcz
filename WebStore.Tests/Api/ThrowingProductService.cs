using System.Linq.Expressions;
using WebStore.Model.DataModels;
using WebStore.Services.Interfaces;
using WebStore.ViewModels.VM;

namespace WebStore.Tests.Api;

// Usługa zgłaszająca nieoczekiwany wyjątek z wewnętrznym szczegółem, który nie może trafić do klienta.
public sealed class ThrowingProductService : IProductService
{
    public const string Secret = "szczegol-wewnetrzny-bazy";

    public ProductVm AddOrUpdateProduct(AddOrUpdateProductVm model) => throw Failure();

    public ProductVm? GetProduct(Expression<Func<Product, bool>> filter) => throw Failure();

    public IEnumerable<ProductVm> GetProducts(Expression<Func<Product, bool>>? filter = null) => throw Failure();

    public void HideProduct(int id) => throw Failure();

    private static InvalidOperationException Failure() => new(Secret);
}
