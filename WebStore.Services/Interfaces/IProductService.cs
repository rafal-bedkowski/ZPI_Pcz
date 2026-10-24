using System.Linq.Expressions;
using WebStore.Model.DataModels;
using WebStore.ViewModels.VM;

namespace WebStore.Services.Interfaces;

public interface IProductService
{
    ProductVm AddOrUpdateProduct(AddOrUpdateProductVm model);
    ProductVm? GetProduct(Expression<Func<Product, bool>> filter);
    IEnumerable<ProductVm> GetProducts(Expression<Func<Product, bool>>? filter = null);

    /// <summary>Ukrywa produkt (flaga, rekord zostaje w bazie). Zgłasza <see cref="KeyNotFoundException"/>, gdy produkt nie istnieje lub jest już ukryty.</summary>
    void HideProduct(int id);
}
